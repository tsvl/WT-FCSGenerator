//! Parser for weapon module .blkx files.
//!
//! Extracts projectile data: mass, caliber, speed, Cx, explosive, DeMarre params, etc.
//!
//! # Legacy Behavior
//!
//! The legacy tool processes weapon files line-by-line with bracket counting that
//! depends on indentation. Due to a quirk in the legacy code:
//!
//! - **Top-level bullets** (2-space indent): Bracket increments for `{`, so the loop
//!   reads through ALL array elements and merges with last-wins semantics.
//!
//! - **Belt bullets** (4-space indent): Bracket doesn't increment for first `{` because
//!   the check for `"    \"bullet\":"` matches, so only the FIRST bullet is read.
//!
//! This explains why the top-level bullet array [UBR6, UOF8] outputs UOF8 (last-wins),
//! while belt arrays like AP [UBR6, UBR6, UBR6, UOR6] output UBR6 (first only).
//!
//! Belt sections (like "30mm_2a42_HE") are filtered based on whether that belt
//! name exists in the vehicle data. Top-level bullets are always included.

use serde_json::Value;

use crate::error::Result;
use crate::model::{ArmorPowerSeries, DemarreParams, Projectile};

/// Parse a weapon module .blkx file and extract projectile data.
///
/// # Arguments
/// * `json` - The parsed JSON content of the weapon module file
/// * `vehicle_json` - Optional vehicle JSON for belt filtering. If `None`, all belts included.
///
/// # Returns
/// A vector of `Projectile` structs extracted from the module.
pub fn parse_weapon_module(json: &Value, vehicle_json: Option<&Value>) -> Result<Vec<Projectile>> {
	let mut projectiles = Vec::new();

	// Convert vehicle JSON to string for belt membership checks (matches legacy behavior)
	let vehicle_str = vehicle_json.map(|v| v.to_string());

	// Process the JSON object in insertion order (serde_json preserves order)
	if let Value::Object(obj) = json {
		for (key, value) in obj {
			match key.as_str() {
				"bullet" => {
					// Top-level bullet - merge all array elements (last-wins)
					if let Some(proj) = collect_bullet_merged(value) {
						projectiles.push(proj);
					}
				},
				"rocket" => {
					// Top-level rocket - merge all array elements (last-wins)
					if let Some(proj) = collect_bullet_merged(value) {
						projectiles.push(proj);
					}
				},
				_ => {
					// Could be a belt section - check if it should be included
					// Belts with rocket/ATGM data are always included (they're not
					// modification-gated); regular ammo belts are filtered by vehicle data.
					let include = belt_has_rocket(value)
						|| (value.is_object()
							&& should_include_belt(key, vehicle_str.as_deref()));
					if include {
						// Look for bullet/rocket within this belt section
						if let Value::Object(belt) = value {
							if let Some(bullets) = belt.get("bullet") {
								// Belt bullets - only first element (legacy bracket behavior)
								if let Some(proj) = collect_bullet_first(bullets) {
									projectiles.push(proj);
								}
							}
							if let Some(rockets) = belt.get("rocket") {
								// Belt rockets - only first element
								if let Some(proj) = collect_bullet_first(rockets) {
									projectiles.push(proj);
								}
							}
						}
					}
				},
			}
		}
	}

	Ok(projectiles)
}

/// Collect ONE bullet from an array, merging ALL elements with last-wins semantics.
/// Used for top-level bullets where legacy bracket counting reads the entire array.
fn collect_bullet_merged(value: &Value) -> Option<Projectile> {
	match value {
		Value::Array(arr) => {
			// Merge all array entries, last wins
			let mut merged = MergedBullet::default();
			for bullet in arr {
				merged.merge(bullet);
			}
			merged.to_projectile()
		},
		Value::Object(_) => {
			let mut merged = MergedBullet::default();
			merged.merge(value);
			merged.to_projectile()
		},
		_ => None,
	}
}

/// Collect ONE bullet from an array, using ONLY the first element.
/// Used for belt bullets where legacy bracket counting exits after first bullet.
fn collect_bullet_first(value: &Value) -> Option<Projectile> {
	match value {
		Value::Array(arr) => {
			// Take ONLY the first element (legacy behavior for belts)
			if let Some(first) = arr.first() {
				let mut merged = MergedBullet::default();
				merged.merge(first);
				merged.to_projectile()
			} else {
				None
			}
		},
		Value::Object(_) => {
			let mut merged = MergedBullet::default();
			merged.merge(value);
			merged.to_projectile()
		},
		_ => None,
	}
}

/// Helper struct for merging bullet values with last-wins semantics.
#[derive(Default)]
struct MergedBullet {
	bullet_name: Option<String>,
	bullet_type: Option<String>,
	mass: Option<f64>,
	caliber: Option<f64>,
	speed: Option<f64>,
	cx: Option<f64>,
	explosive_mass: Option<f64>,
	explosive_type: Option<String>,
	damage_mass: Option<f64>,
	damage_caliber: Option<f64>,
	demarre_k: Option<f64>,
	demarre_speed_pow: Option<f64>,
	demarre_mass_pow: Option<f64>,
	demarre_caliber_pow: Option<f64>,
	armor_power: Option<f64>,
	armorpower_json: Option<Value>, // Store raw armorpower section for APDS extraction
}

impl MergedBullet {
	/// Merge values from a bullet object, overwriting any existing values.
	fn merge(&mut self, bullet: &Value) {
		// For rockets/ATGMs, data may be nested under "rocket"
		let rocket = bullet.get("rocket");
		let data_source = rocket.unwrap_or(bullet);

		// Extract bullet name (can be string or array)
		if let Some(name) = extract_bullet_name(bullet) {
			self.bullet_name = Some(name);
		}

		if let Some(bt) = bullet.get("bulletType").and_then(Value::as_str) {
			self.bullet_type = Some(bt.to_string());
		}
		// Legacy scans line-by-line with last-wins, so rocket.bulletType overwrites outer
		if let Some(bt) = bullet
			.get("rocket")
			.and_then(|r| r.get("bulletType"))
			.and_then(Value::as_str)
		{
			self.bullet_type = Some(bt.to_string());
		}

		// Mass - try rocket section first, then bullet level
		if let Some(v) = data_source.get("mass").and_then(Value::as_f64) {
			self.mass = Some(v);
		} else if let Some(v) = bullet.get("mass").and_then(Value::as_f64) {
			self.mass = Some(v);
		}

		// Caliber - prefer ballisticCaliber, then caliber
		if let Some(v) = data_source.get("ballisticCaliber").and_then(Value::as_f64) {
			self.caliber = Some(v);
		} else if let Some(v) = data_source.get("caliber").and_then(Value::as_f64) {
			self.caliber = Some(v);
		} else if let Some(v) = bullet.get("caliber").and_then(Value::as_f64) {
			self.caliber = Some(v);
		}

		// Speed - prefer endSpeed, then speed
		if let Some(v) = data_source.get("endSpeed").and_then(Value::as_f64) {
			self.speed = Some(v);
		} else if let Some(v) = data_source.get("speed").and_then(Value::as_f64) {
			self.speed = Some(v);
		}

		// Cx
		if let Some(cx) = extract_cx(data_source) {
			self.cx = Some(cx);
		}

		// Explosive
		if let Some(v) = data_source.get("explosiveMass").and_then(Value::as_f64) {
			self.explosive_mass = Some(v);
		}
		if let Some(v) = data_source.get("explosiveType").and_then(Value::as_str) {
			self.explosive_type = Some(v.to_string());
		}

		// Damage
		if let Some(v) = data_source.get("damageMass").and_then(Value::as_f64) {
			self.damage_mass = Some(v);
		}
		if let Some(v) = extract_f64_or_first(data_source, "damageCaliber") {
			self.damage_caliber = Some(v);
		} else if let Some(v) = extract_f64_or_first(bullet, "damageCaliber") {
			self.damage_caliber = Some(v);
		}

		// DeMarre - check bullet level, damage.kinetic, and rocket.damage.kinetic
		self.merge_demarre(bullet);

		// Armor power
		if let Some(v) = extract_armor_power(bullet) {
			self.armor_power = Some(v);
		}

		// Store armorpower section for APDS series extraction
		if let Some(ap) = bullet.get("armorpower") {
			self.armorpower_json = Some(ap.clone());
		}
	}

	fn merge_demarre(&mut self, bullet: &Value) {
		let rocket_damage_kinetic = bullet
			.get("rocket")
			.and_then(|r| r.get("damage"))
			.and_then(|d| d.get("kinetic"));
		let bullet_damage_kinetic = bullet
			.get("damage")
			.and_then(|d| d.get("kinetic"));

		let sources = [
			Some(bullet),
			bullet_damage_kinetic,
			rocket_damage_kinetic,
		];

		for source in sources.into_iter().flatten() {
			if let Some(v) = source.get("demarrePenetrationK").and_then(Value::as_f64) {
				self.demarre_k = Some(v);
			}
			if let Some(v) = source.get("demarreSpeedPow").and_then(Value::as_f64) {
				self.demarre_speed_pow = Some(v);
			}
			if let Some(v) = source.get("demarreMassPow").and_then(Value::as_f64) {
				self.demarre_mass_pow = Some(v);
			}
			if let Some(v) = source.get("demarreCaliberPow").and_then(Value::as_f64) {
				self.demarre_caliber_pow = Some(v);
			}
		}
	}

	fn to_projectile(self) -> Option<Projectile> {
		// Name and type are required
		let name = self.bullet_name?;
		let bullet_type = self.bullet_type?;

		let demarre = if self.demarre_k.is_some()
			|| self.demarre_speed_pow.is_some()
			|| self.demarre_mass_pow.is_some()
			|| self.demarre_caliber_pow.is_some()
		{
			Some(DemarreParams {
				k: self.demarre_k.unwrap_or(0.9),
				speed_pow: self.demarre_speed_pow.unwrap_or(1.43),
				mass_pow: self.demarre_mass_pow.unwrap_or(0.71),
				caliber_pow: self.demarre_caliber_pow.unwrap_or(1.07),
			})
		} else {
			None
		};

		// Armor power series for APDS/APFSDS
		let armor_power_series = if bullet_type.starts_with("apds") {
			self.armorpower_json
				.as_ref()
				.map(extract_armor_power_series)
		} else {
			None
		};

		Some(Projectile {
			name,
			bullet_type,
			mass: self.mass,
			ballistic_caliber: self.caliber,
			speed: self.speed,
			cx: self.cx,
			explosive_mass: self.explosive_mass,
			explosive_type: self.explosive_type,
			damage_mass: self.damage_mass,
			damage_caliber: self.damage_caliber,
			demarre,
			armor_power: self.armor_power,
			armor_power_series,
		})
	}
}

/// Check if a belt section contains rocket/ATGM data (nested `rocket` inside `bullet`).
/// Such belts represent gun-launched ATGMs or missiles and should always be included
/// regardless of belt filtering, since they're not modification-gated ammo belts.
fn belt_has_rocket(value: &Value) -> bool {
	if let Value::Object(belt) = value {
		// Check bullet sub-objects for a nested "rocket" section
		if let Some(bullets) = belt.get("bullet") {
			let bullet_iter: Box<dyn Iterator<Item = &Value>> = match bullets {
				Value::Array(arr) => Box::new(arr.iter()),
				obj @ Value::Object(_) => Box::new(std::iter::once(obj)),
				_ => return false,
			};
			for bullet in bullet_iter {
				if bullet.get("rocket").is_some() {
					return true;
				}
			}
		}
		// Check for direct "rocket" key in belt (standalone rocket sections)
		if let Some(rockets) = belt.get("rocket") {
			match rockets {
				Value::Object(_) | Value::Array(_) => return true,
				_ => {}
			}
		}
	}
	false
}

/// Check if a belt should be included based on vehicle data.
fn should_include_belt(belt_name: &str, vehicle_str: Option<&str>) -> bool {
	let vehicle_str = match vehicle_str {
		Some(s) => s,
		None => return true, // No vehicle data, include all
	};

	// Direct check
	if vehicle_str.contains(belt_name) {
		return true;
	}

	// Try with nation prefixes stripped (legacy behavior)
	let normalized = strip_nation_prefix(belt_name);
	if normalized != belt_name && vehicle_str.contains(&normalized) {
		return true;
	}

	false
}

/// Strip nation prefix from belt name (legacy behavior).
fn strip_nation_prefix(name: &str) -> String {
	let prefixes = [
		"_cn_", "_fr_", "_germ_", "_il_", "_it_", "_jp_", "_sw_", "_uk_", "_us_", "_ussr_",
	];

	let mut result = name.to_string();
	for prefix in prefixes {
		result = result.replace(prefix, "_");
	}
	result
}

/// Extract bullet name, handling both scalar and array cases.
/// When no bulletName exists, fallback to bulletType + "/name/short" (legacy behavior).
fn extract_bullet_name(bullet: &Value) -> Option<String> {
	match bullet.get("bulletName") {
		Some(Value::String(s)) => Some(s.clone()),
		Some(Value::Array(arr)) => arr.first().and_then(Value::as_str).map(String::from),
		_ => {
			// Fallback: bulletType + "/name/short" (legacy behavior)
			bullet
				.get("bulletType")
				.and_then(Value::as_str)
				.map(|s| format!("{s}/name/short"))
		},
	}
}

/// Extract Cx drag coefficient, handling both scalar and array cases.
/// For arrays, legacy tool averages all values and rounds to 4 decimal places.
fn extract_cx(obj: &Value) -> Option<f64> {
	match obj.get("Cx") {
		Some(Value::Number(n)) => n.as_f64(),
		Some(Value::Array(arr)) => {
			// Legacy behavior: average all array values, round to 4 decimal places
			let values: Vec<f64> = arr.iter().filter_map(Value::as_f64).collect();
			if values.is_empty() {
				None
			} else {
				let avg = values.iter().sum::<f64>() / values.len() as f64;
				// Math.Round(average, 4) in C#
				Some((avg * 10000.0).round() / 10000.0)
			}
		},
		_ => None,
	}
}

/// Extract a float value from a key, handling both scalar and array cases.
/// For arrays, takes the first element (matching legacy line-scan behavior).
fn extract_f64_or_first(obj: &Value, key: &str) -> Option<f64> {
	match obj.get(key) {
		Some(Value::Number(n)) => n.as_f64(),
		Some(Value::Array(arr)) => arr.first().and_then(Value::as_f64),
		_ => None,
	}
}

/// Extract armor power for ATGMs/rockets.
fn extract_armor_power(bullet: &Value) -> Option<f64> {
	// Check multiple possible locations
	let sources = [
		bullet.get("cumulativeDamage"),
		bullet.get("rocket").and_then(|r| r.get("cumulativeDamage")),
		Some(bullet),
	];

	for source in sources.into_iter().flatten() {
		if let Some(v) = source.get("armorPower").and_then(Value::as_f64) {
			return Some(v);
		}
	}

	None
}

/// Extract armor power series from armorpower JSON object.
/// The armorpower section has keys like "ArmorPower0m", "ArmorPower100m", etc.
/// Values are arrays [penetration, distance], we only need the penetration (first element).
fn extract_armor_power_series(armorpower: &Value) -> ArmorPowerSeries {
	/// Extract penetration value from ArmorPowerXXXm array.
	fn get_ap(obj: &Value, key: &str) -> Option<f64> {
		obj.get(key).and_then(|v| match v {
			Value::Array(arr) => arr.first().and_then(Value::as_f64),
			Value::Number(n) => n.as_f64(),
			_ => None,
		})
	}

	ArmorPowerSeries {
		ap_0m: get_ap(armorpower, "ArmorPower0m"),
		ap_100m: get_ap(armorpower, "ArmorPower100m"),
		ap_500m: get_ap(armorpower, "ArmorPower500m"),
		ap_1000m: get_ap(armorpower, "ArmorPower1000m"),
		ap_1500m: get_ap(armorpower, "ArmorPower1500m"),
		ap_2000m: get_ap(armorpower, "ArmorPower2000m"),
		ap_2500m: get_ap(armorpower, "ArmorPower2500m"),
		ap_3000m: get_ap(armorpower, "ArmorPower3000m"),
		ap_3500m: get_ap(armorpower, "ArmorPower3500m"),
		ap_4000m: get_ap(armorpower, "ArmorPower4000m"),
		ap_4500m: get_ap(armorpower, "ArmorPower4500m"),
		ap_10000m: get_ap(armorpower, "ArmorPower10000m"),
	}
}

#[cfg(test)]
mod tests {
	use super::*;
	use serde_json::json;

	#[test]
	fn test_extract_cx_array() {
		let obj = json!({ "Cx": [0.3, 0.4, 0.5] });
		let cx = extract_cx(&obj);
		// Should average all values: (0.3 + 0.4 + 0.5) / 3 = 0.4, rounded to 4 decimal places
		assert!((cx.unwrap() - 0.4).abs() < 0.00001);

		// Test rounding to 4 decimal places
		let obj2 = json!({ "Cx": [0.276611, 0.33] });
		let cx2 = extract_cx(&obj2);
		// Average = 0.303305... rounds to 0.3033
		assert!((cx2.unwrap() - 0.3033).abs() < 0.00001);
	}

	#[test]
	fn test_extract_bullet_name_array() {
		let bullet = json!({
			"bulletName": ["first_name", "second_name"],
			"bulletType": "ap_t"
		});
		let name = extract_bullet_name(&bullet);
		assert_eq!(name, Some("first_name".to_string()));
	}

	#[test]
	fn test_belt_filtering() {
		let weapon = json!({
			"bullet": [{
				"bulletName": "top_level",
				"bulletType": "ap"
			}],
			"30mm_HE": {
				"bullet": [{
					"bulletName": "belt_bullet",
					"bulletType": "he"
				}]
			}
		});

		// No vehicle filter - should include both
		let result = parse_weapon_module(&weapon, None).unwrap();
		assert_eq!(result.len(), 2);

		// With vehicle filter that includes 30mm_HE
		let vehicle = json!({"30mm_HE": {}});
		let result = parse_weapon_module(&weapon, Some(&vehicle)).unwrap();
		assert_eq!(result.len(), 2);

		// With vehicle filter that excludes 30mm_HE
		let vehicle = json!({"other_belt": {}});
		let result = parse_weapon_module(&weapon, Some(&vehicle)).unwrap();
		assert_eq!(result.len(), 1);
		assert_eq!(result[0].name, "top_level");
	}

	#[test]
	fn test_belt_with_rocket_always_included() {
		let weapon = json!({
			"bullet": [{
				"bulletName": "top_level",
				"bulletType": "ap"
			}],
			"125mm_china_ATGM": {
				"bullet": {
					"bulletType": "atgm_tandem_tank",
					"mass": 19.0,
					"caliber": 0.125,
					"speed": 470.0,
					"rocket": {
						"mass": 19.0,
						"caliber": 0.125,
						"endSpeed": 470.0,
						"explosiveMass": 3.6,
						"explosiveType": "ocfol"
					}
				}
			}
		});

		// Even with a vehicle that has NO matching belt, the ATGM belt should be included
		let vehicle = json!({"125mm_china_HE": {}});
		let result = parse_weapon_module(&weapon, Some(&vehicle)).unwrap();
		assert_eq!(result.len(), 2, "ATGM belt should be included: {:?}", result);
		assert_eq!(result[1].bullet_type, "atgm_tandem_tank");
	}
}
