//! Parser for unittags.blkx — the authoritative source for vehicle IDs with correct casing.
//!
//! War Thunder's `char.vromfs.bin/config/unittags.blkx` contains vehicle definitions
//! as a JSON object where keys are the full vehicle IDs (e.g., `"germ_pzkpfw_VI_ausf_h1_tiger"`).
//!
//! This module builds a lookup table from lowercase vehicle IDs to their correct-casing
//! counterparts, which is required for War Thunder's case-sensitive UserSights folder matching.

use std::collections::HashMap;

use serde_json::Value;

/// A map from lowercase vehicle ID to correctly-cased vehicle ID.
///
/// Example: `"germ_pzkpfw_vi_ausf_h1_tiger"` → `"germ_pzkpfw_VI_ausf_h1_tiger"`
pub type UnittagsMap = HashMap<String, String>;

/// Parse unittags.blkx JSON and build a lookup map.
///
/// The JSON structure is an object where each key is a vehicle/unit ID:
/// ```json
/// {
///     "germ_pzkpfw_VI_ausf_h1_tiger": { ... },
///     "cn_ztz_99a": { ... },
///     ...
/// }
/// ```
///
/// Returns a map of lowercase ID → original ID with correct casing.
pub fn parse_unittags(json: &Value) -> UnittagsMap {
	let mut map = HashMap::new();

	if let Value::Object(obj) = json {
		for key in obj.keys() {
			// Only include tank vehicles (ground vehicles have nation prefixes)
			// Skip ships, aircraft, etc. that don't follow the nation_vehicle pattern
			// Common nation prefixes: us_, germ_, ussr_, uk_, jp_, cn_, it_, fr_, sw_, il_
			// But we include ALL keys - filtering happens at lookup time
			map.insert(key.to_lowercase(), key.clone());
		}
	}

	map
}

/// Parse unittags from a JSON string.
///
/// Returns `None` if the JSON is invalid.
pub fn parse_unittags_str(content: &str) -> Option<UnittagsMap> {
	let json: Value = serde_json::from_str(content).ok()?;
	Some(parse_unittags(&json))
}

/// Look up the correctly-cased vehicle ID.
///
/// Returns the correct-casing ID if found, otherwise returns the input unchanged.
pub fn lookup_vehicle_id<'a>(map: &'a UnittagsMap, id: &'a str) -> &'a str {
	map.get(&id.to_lowercase())
		.map(String::as_str)
		.unwrap_or(id)
}

#[cfg(test)]
mod tests {
	use super::*;
	use serde_json::json;

	#[test]
	fn test_parse_unittags() {
		let json = json!({
			"germ_pzkpfw_VI_ausf_h1_tiger": {},
			"cn_ztz_99a": {},
			"us_m1a2_sep_abrams": {},
			"jp_type_90": {}
		});

		let map = parse_unittags(&json);

		assert_eq!(map.len(), 4);
		assert_eq!(
			map.get("germ_pzkpfw_vi_ausf_h1_tiger"),
			Some(&"germ_pzkpfw_VI_ausf_h1_tiger".to_string())
		);
		assert_eq!(map.get("cn_ztz_99a"), Some(&"cn_ztz_99a".to_string()));
		assert_eq!(
			map.get("us_m1a2_sep_abrams"),
			Some(&"us_m1a2_sep_abrams".to_string())
		);
	}

	#[test]
	fn test_lookup_vehicle_id() {
		let json = json!({
			"germ_pzkpfw_VI_ausf_b_tiger_IIh_sla": {},
			"cn_ztz_99a": {}
		});
		let map = parse_unittags(&json);

		// Exact lowercase match returns correct casing
		assert_eq!(
			lookup_vehicle_id(&map, "germ_pzkpfw_vi_ausf_b_tiger_iih_sla"),
			"germ_pzkpfw_VI_ausf_b_tiger_IIh_sla"
		);

		// Already correct casing still works
		assert_eq!(
			lookup_vehicle_id(&map, "germ_pzkpfw_VI_ausf_b_tiger_IIh_sla"),
			"germ_pzkpfw_VI_ausf_b_tiger_IIh_sla"
		);

		// Missing ID returns input unchanged
		assert_eq!(
			lookup_vehicle_id(&map, "unknown_vehicle"),
			"unknown_vehicle"
		);
	}

	#[test]
	fn test_parse_unittags_str() {
		let content = r#"{"germ_pzkpfw_VI_ausf_h1_tiger": {}, "cn_ztz_99a": {}}"#;
		let map = parse_unittags_str(content).unwrap();

		assert_eq!(map.len(), 2);
		assert_eq!(
			map.get("germ_pzkpfw_vi_ausf_h1_tiger"),
			Some(&"germ_pzkpfw_VI_ausf_h1_tiger".to_string())
		);
	}

	#[test]
	fn test_parse_unittags_str_invalid_json() {
		assert!(parse_unittags_str("invalid json").is_none());
	}
}
