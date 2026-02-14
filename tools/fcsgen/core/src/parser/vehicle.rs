//! Parser for vehicle .blkx files.
//!
//! Extracts header information: weapon paths, rocket paths, zoom values, and laser presence.

use serde_json::Value;

use crate::error::Result;
use crate::model::VehicleData;

/// Parsed weapon entry from a vehicle file.
#[derive(Debug, Clone)]
pub struct WeaponEntry {
	pub blk_path: String,
	pub trigger: Option<String>,
	pub trigger_group: Option<String>,
}

/// Parse a vehicle .blkx file and extract header information.
///
/// # Arguments
/// * `json` - The parsed JSON content of the vehicle file
/// * `vehicle_id` - The vehicle identifier (basename without extension)
///
/// # Returns
/// A `VehicleData` struct with header fields populated (but no projectiles yet).
pub fn parse_vehicle(json: &Value, vehicle_id: &str) -> Result<VehicleData> {
	let mut data = VehicleData::new(vehicle_id);

	// Extract zoom values from cockpit
	if let Some(cockpit) = json.get("cockpit") {
		extract_zoom_values(cockpit, &mut data);
	}

	// Check for laser rangefinder (broad heuristic matching legacy behavior)
	data.has_laser = check_has_laser(json);

	// Extract weapon paths from top-level commonWeapons
	if let Some(common_weapons) = json.get("commonWeapons") {
		let weapons = extract_weapon_entries(common_weapons);
		classify_weapons(&weapons, &mut data);
	}

	// Also scan modifications for effects.commonWeapons blocks
	// These contain weapons unlocked by modifications (e.g., upgraded ATGMs, different guns)
	if let Some(Value::Object(modifications)) = json.get("modifications") {
		for (_mod_name, mod_value) in modifications {
			if let Some(common_weapons) = mod_value
				.get("effects")
				.and_then(|e| e.get("commonWeapons"))
			{
				let weapons = extract_weapon_entries(common_weapons);
				classify_weapons(&weapons, &mut data);
			}
		}
	}

	Ok(data)
}

/// Extract zoom values from the cockpit object.
fn extract_zoom_values(cockpit: &Value, data: &mut VehicleData) {
	// Handle both single cockpit object and array of cockpits
	match cockpit {
		Value::Object(obj) => {
			data.zoom_in = extract_fov_value(obj.get("zoomInFov"));
			data.zoom_out = extract_fov_value(obj.get("zoomOutFov"));
		},
		Value::Array(arr) => {
			// First cockpit is primary
			if let Some(Value::Object(obj)) = arr.first() {
				data.zoom_in = extract_fov_value(obj.get("zoomInFov"));
				data.zoom_out = extract_fov_value(obj.get("zoomOutFov"));
			}
			// Second cockpit is secondary optics
			if let Some(Value::Object(obj)) = arr.get(1) {
				data.zoom_in_2 = extract_fov_value(obj.get("zoomInFov"));
				data.zoom_out_2 = extract_fov_value(obj.get("zoomOutFov"));
			}
		},
		_ => {},
	}
}

/// Extract FOV value, handling both scalar and array cases.
fn extract_fov_value(value: Option<&Value>) -> Option<f64> {
	match value {
		Some(Value::Number(n)) => n.as_f64(),
		Some(Value::Array(arr)) => {
			// Take first numeric value from array
			arr.iter().find_map(|v| v.as_f64())
		},
		_ => None,
	}
}

/// Check if the vehicle has a laser rangefinder.
/// Uses broad substring matching to match legacy behavior.
/// Note: Legacy uses case-sensitive matching, so "LaserBeamRidingSensor" does NOT match.
///
/// TODO: This is a very crude heuristic that also matches laser warning systems (LWS),
/// thermal imaging systems with "laser" in their names, etc. In practice this usually
/// works because vehicles with LWS typically also have a laser rangefinder, but ideally
/// we should look for specific fields like "modern_tank_laser_rangefinder" modification
/// or the "isLaser" field in the modifications section to properly detect this.
fn check_has_laser(json: &Value) -> bool {
	// Case-sensitive search for "laser" substring
	// Legacy C# uses String.Contains which is case-sensitive by default
	// This means "LaserBeamRidingSensor" (missile guidance) doesn't trigger it,
	// but "modern_tank_laser_rangefinder" or "isLaser" does.
	let json_str = json.to_string();
	json_str.contains("laser")
}

/// Extract weapon entries from commonWeapons.
fn extract_weapon_entries(common_weapons: &Value) -> Vec<WeaponEntry> {
	let mut entries = Vec::new();

	let weapons = match common_weapons.get("Weapon") {
		Some(Value::Array(arr)) => arr.as_slice(),
		Some(obj @ Value::Object(_)) => std::slice::from_ref(obj),
		_ => return entries,
	};

	for weapon in weapons {
		if let Some(blk) = weapon.get("blk").and_then(Value::as_str) {
			entries.push(WeaponEntry {
				blk_path: normalize_blk_path(blk),
				trigger: weapon
					.get("trigger")
					.and_then(Value::as_str)
					.map(String::from),
				trigger_group: weapon
					.get("triggerGroup")
					.and_then(Value::as_str)
					.map(String::from),
			});
		}
	}

	entries
}

/// Normalize .blk path to .blkx (legacy behavior: append 'x').
fn normalize_blk_path(path: &str) -> String {
	if path.ends_with(".blk") {
		format!("{path}x")
	} else {
		path.to_string()
	}
}

/// Classify weapons into primary weapon and rocket paths.
///
/// Legacy behavior:
/// - First weapon with "groundModels_weapons" in path becomes weapon_path
/// - Weapons with triggerGroup "special" become rocket_paths (up to 2 unique)
fn classify_weapons(weapons: &[WeaponEntry], data: &mut VehicleData) {
	// Find primary weapon (first one with groundModels_weapons that isn't special)
	for weapon in weapons {
		if weapon.blk_path.contains("groundModels_weapons")
			&& weapon.trigger_group.as_deref() != Some("special")
			&& data.weapon_path.is_none()
		{
			data.weapon_path = Some(weapon.blk_path.clone());
			break;
		}
	}

	// Find rocket paths (triggerGroup == "special")
	for weapon in weapons {
		if weapon.trigger_group.as_deref() == Some("special") {
			if !data.rocket_paths.contains(&weapon.blk_path) && data.rocket_paths.len() < 2 {
				data.rocket_paths.push(weapon.blk_path.clone());
			}
		}
	}
}

#[cfg(test)]
mod tests {
	use super::*;
	use serde_json::json;

	#[test]
	fn test_extract_zoom_scalar() {
		let cockpit = json!({
			"zoomInFov": 6.14,
			"zoomOutFov": 29.8
		});
		let mut data = VehicleData::new("test");
		extract_zoom_values(&cockpit, &mut data);

		assert!((data.zoom_in.unwrap() - 6.14).abs() < 0.001);
		assert!((data.zoom_out.unwrap() - 29.8).abs() < 0.001);
	}

	#[test]
	fn test_extract_zoom_array() {
		let cockpit = json!({
			"zoomInFov": [6.0, 8.0, 10.0],
			"zoomOutFov": [30.0, 40.0]
		});
		let mut data = VehicleData::new("test");
		extract_zoom_values(&cockpit, &mut data);

		// Should take first value from arrays
		assert!((data.zoom_in.unwrap() - 6.0).abs() < 0.001);
		assert!((data.zoom_out.unwrap() - 30.0).abs() < 0.001);
	}

	#[test]
	fn test_normalize_blk_path() {
		assert_eq!(
			normalize_blk_path("gameData/Weapons/test.blk"),
			"gameData/Weapons/test.blkx"
		);
		assert_eq!(
			normalize_blk_path("gameData/Weapons/test.blkx"),
			"gameData/Weapons/test.blkx"
		);
	}
}
