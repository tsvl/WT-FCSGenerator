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
use crate::model::{DemarreParams, Projectile};

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
                }
                "rocket" => {
                    // Top-level rocket - merge all array elements (last-wins)
                    if let Some(proj) = collect_bullet_merged(value) {
                        projectiles.push(proj);
                    }
                }
                _ => {
                    // Could be a belt section - check if it should be included
                    if value.is_object() && should_include_belt(key, vehicle_str.as_deref()) {
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
                }
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
        }
        Value::Object(_) => {
            let mut merged = MergedBullet::default();
            merged.merge(value);
            merged.to_projectile()
        }
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
        }
        Value::Object(_) => {
            let mut merged = MergedBullet::default();
            merged.merge(value);
            merged.to_projectile()
        }
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
        if let Some(v) = data_source.get("damageCaliber").and_then(Value::as_f64) {
            self.damage_caliber = Some(v);
        }

        // DeMarre - check bullet level and damage.kinetic
        self.merge_demarre(bullet);

        // Armor power
        if let Some(v) = extract_armor_power(bullet) {
            self.armor_power = Some(v);
        }
    }

    fn merge_demarre(&mut self, bullet: &Value) {
        let sources = [
            bullet,
            bullet
                .get("damage")
                .and_then(|d| d.get("kinetic"))
                .unwrap_or(&Value::Null),
        ];

        for source in sources {
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

        // Armor power series for APDS
        let armor_power_series = if bullet_type.starts_with("apds") {
            // Would need to extract from damage section - leaving None for now
            // as we need the original bullet JSON for that
            None
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
fn extract_bullet_name(bullet: &Value) -> Option<String> {
    match bullet.get("bulletName") {
        Some(Value::String(s)) => Some(s.clone()),
        Some(Value::Array(arr)) => arr.first().and_then(Value::as_str).map(String::from),
        _ => {
            // Fallback: bulletType + short name (legacy behavior)
            bullet
                .get("bulletType")
                .and_then(Value::as_str)
                .map(String::from)
        }
    }
}

/// Extract Cx drag coefficient, handling both scalar and array cases.
fn extract_cx(obj: &Value) -> Option<f64> {
    match obj.get("Cx") {
        Some(Value::Number(n)) => n.as_f64(),
        Some(Value::Array(arr)) => {
            // Take first value (simpler than averaging, handles the rare array case)
            arr.first().and_then(Value::as_f64)
        }
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

#[cfg(test)]
mod tests {
    use super::*;
    use serde_json::json;

    #[test]
    fn test_extract_cx_array() {
        let obj = json!({ "Cx": [0.3, 0.4, 0.5] });
        let cx = extract_cx(&obj);
        // Should take first value
        assert!((cx.unwrap() - 0.3).abs() < 0.001);
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
}
