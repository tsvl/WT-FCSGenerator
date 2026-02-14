//! Parser for the legacy `Data/{vehicle}.txt` intermediate format.
//!
//! Reads the key:value text files produced by `fcsgen convert` (Stage 1)
//! back into structured data for use by the ballistic computation (Stage 2).

use std::collections::HashMap;
use std::path::Path;

use crate::model::{ArmorPowerSeries, Projectile};

/// Parsed vehicle data from a `Data/{vehicle}.txt` file.
#[derive(Debug, Clone)]
pub struct DataFile {
	/// Vehicle identifier (filename stem).
	pub vehicle_id: String,

	/// Path to the primary weapon module.
	pub weapon_path: Option<String>,

	/// Rocket/ATGM module paths.
	pub rocket_paths: Vec<String>,

	/// Primary optics zoom (narrow FOV).
	pub zoom_in: Option<f64>,

	/// Primary optics zoom (wide FOV).
	pub zoom_out: Option<f64>,

	/// Whether the vehicle has a laser rangefinder.
	pub has_laser: bool,

	/// Parsed projectile blocks.
	pub projectiles: Vec<DataProjectile>,
}

/// A single projectile block from a `Data/{vehicle}.txt` file.
#[derive(Debug, Clone)]
pub struct DataProjectile {
	/// Full projectile name (e.g. `105mm_m735`).
	pub name: String,

	/// Raw type string (e.g. `apds_fs_tungsten_l10_l15_tank`).
	pub bullet_type: String,

	/// Normalized type for ballistic computation.
	///
	/// If the raw type contains `apds_fs`, this is `apds_fs`.
	/// Otherwise it is the first `_`-delimited segment (e.g. "apcbc", "he", "heat").
	pub normalized_type: String,

	/// Projectile mass in kg.
	pub mass: f64,

	/// Ballistic caliber in meters.
	pub ballistic_caliber: f64,

	/// Muzzle velocity in m/s.
	pub speed: f64,

	/// Drag coefficient.
	pub cx: f64,

	/// Explosive filler mass in kg.
	pub explosive_mass: f64,

	/// Sub-caliber core mass in kg (APCR/APDS).
	pub damage_mass: f64,

	/// Sub-caliber core caliber in meters (APCR/APDS).
	pub damage_caliber: f64,

	/// `DeMarre` penetration coefficient K.
	pub demarre_k: f64,

	/// `DeMarre` speed exponent.
	pub demarre_speed_pow: f64,

	/// `DeMarre` mass exponent.
	pub demarre_mass_pow: f64,

	/// `DeMarre` caliber exponent.
	pub demarre_caliber_pow: f64,

	/// APDS-FS armor power lookup table: `(distance_m, penetration_mm)` pairs.
	///
	/// Populated from `APDS{distance}:{penetration}` lines in the data file.
	/// Empty for non-APDS-FS types.
	pub armor_power_table: Vec<(f64, f64)>,

	/// Shell name cleaned for output filename.
	///
	/// Caliber prefix (everything up to and including "mm_") is stripped.
	/// Everything after "/" is removed.
	pub output_name: String,
}

/// Parse a `Data/{vehicle}.txt` file from disk.
///
/// # Errors
///
/// Returns an error if the file cannot be read.
pub fn parse_data_file(path: &Path) -> std::io::Result<DataFile> {
	let content = std::fs::read_to_string(path)?;
	let vehicle_id = path
		.file_stem()
		.and_then(|s| s.to_str())
		.unwrap_or("unknown")
		.to_owned();

	Ok(parse_data_text(&content, &vehicle_id))
}

/// Parse a `Data/{vehicle}.txt` from a string.
#[must_use]
pub fn parse_data_text(content: &str, vehicle_id: &str) -> DataFile {
	let content = content.replace("\r\n", "\n");
	let mut weapon_path = None;
	let mut rocket_paths = Vec::new();
	let mut zoom_in = None;
	let mut zoom_out = None;
	let mut has_laser = false;
	let mut projectiles = Vec::new();

	// Split into sections by blank lines
	let sections: Vec<&str> = content.split("\n\n").collect();

	// First section is the header
	if let Some(header) = sections.first() {
		for line in header.lines() {
			let line = line.trim();
			if let Some((key, value)) = line.split_once(':') {
				match key {
					"WeaponPath" => weapon_path = Some(value.to_owned()),
					"RocketPath" => rocket_paths.push(value.to_owned()),
					"ZoomIn" => zoom_in = value.parse().ok(),
					"ZoomOut" => zoom_out = value.parse().ok(),
					_ => {},
				}
			} else if line == "HasLaser" {
				has_laser = true;
			}
		}
	}

	// Remaining sections are projectile blocks
	for section in sections.iter().skip(1) {
		if let Some(proj) = parse_projectile_block(section) {
			projectiles.push(proj);
		}
	}

	DataFile {
		vehicle_id: vehicle_id.to_owned(),
		weapon_path,
		rocket_paths,
		zoom_in,
		zoom_out,
		has_laser,
		projectiles,
	}
}

/// Parse a single projectile block (lines between blank lines).
fn parse_projectile_block(block: &str) -> Option<DataProjectile> {
	let mut fields: HashMap<&str, &str> = HashMap::new();
	let mut apds_entries: Vec<(f64, f64)> = Vec::new();

	for line in block.lines() {
		let line = line.trim();
		if line.is_empty() {
			continue;
		}
		if let Some((key, value)) = line.split_once(':') {
			// Check for APDS distance-penetration entries (e.g. "APDS0:292.4")
			if key.starts_with("APDS") {
				if let Some(dist_str) = key.strip_prefix("APDS")
					&& let (Ok(dist), Ok(pen)) = (dist_str.parse::<f64>(), value.parse::<f64>())
				{
					apds_entries.push((dist, pen));
				}
			} else {
				fields.insert(key, value);
			}
		}
	}

	let name = (*fields.get("Name")?).to_owned();
	let bullet_type = fields.get("Type").copied().unwrap_or("").to_owned();

	// Normalize type for ballistic computation (matches C# logic)
	let normalized_type = normalize_shell_type(&bullet_type);

	// Parse numeric fields with 0.0 defaults
	let mass = parse_f64(fields.get("BulletMass").copied());
	let ballistic_caliber = parse_f64(fields.get("BallisticCaliber").copied());
	let speed = parse_f64(fields.get("Speed").copied());
	let cx = parse_f64(fields.get("Cx").copied());
	let explosive_mass = parse_f64(fields.get("ExplosiveMass").copied());
	let damage_mass = parse_f64(fields.get("DamageMass").copied());
	let damage_caliber = parse_f64(fields.get("DamageCaliber").copied());
	let demarre_k = parse_f64(fields.get("demarrePenetrationK").copied());
	let demarre_speed_pow = parse_f64(fields.get("demarreSpeedPow").copied());
	let demarre_mass_pow = parse_f64(fields.get("demarreMassPow").copied());
	let demarre_caliber_pow = parse_f64(fields.get("demarreCaliberPow").copied());

	// Sort APDS entries by distance for correct interpolation
	apds_entries.sort_by(|a, b| a.0.partial_cmp(&b.0).unwrap_or(std::cmp::Ordering::Equal));

	// Build output name: strip caliber prefix and path suffix
	let output_name = clean_shell_name(&name);

	Some(DataProjectile {
		name,
		bullet_type,
		normalized_type,
		mass,
		ballistic_caliber,
		speed,
		cx,
		explosive_mass,
		damage_mass,
		damage_caliber,
		demarre_k,
		demarre_speed_pow,
		demarre_mass_pow,
		demarre_caliber_pow,
		armor_power_table: apds_entries,
		output_name,
	})
}

/// Normalize the shell type string for ballistic computation.
///
/// Matches the C# logic:
/// - If the type contains `apds_fs`, return `apds_fs`
/// - Otherwise take the first underscore-delimited segment
#[must_use]
pub fn normalize_shell_type(raw_type: &str) -> String {
	if raw_type.contains("apds_fs") {
		return "apds_fs".to_owned();
	}
	raw_type
		.split('_')
		.next()
		.unwrap_or("")
		.to_owned()
}

/// Clean a shell name for use as an output filename.
///
/// Strips the caliber prefix (everything up to and including "mm_")
/// and removes everything after "/" (matching C# `BulletName` cleanup).
#[must_use]
pub fn clean_shell_name(name: &str) -> String {
	// Strip everything after "/" first
	let name = name.split('/').next().unwrap_or(name);

	// Strip caliber prefix (everything up to and including "mm_")
	if let Some(pos) = name.find("mm_") {
		name[pos + 3..].to_owned()
	} else {
		name.to_owned()
	}
}

/// Parse a string as f64, returning 0.0 on failure or None.
fn parse_f64(s: Option<&str>) -> f64 {
	s.and_then(|v| v.parse().ok()).unwrap_or(0.0)
}

/// Default Cx value for projectiles without an explicit drag coefficient.
///
/// Matches the legacy emitter behaviour: rockets/ATGMs and other shells
/// missing a Cx field get 0.38 written into `Data/*.txt`.
const DEFAULT_CX: f64 = 0.38;

/// Fixed distance steps for the `ArmorPowerSeries` → table conversion.
#[allow(clippy::type_complexity)]
const ARMOR_POWER_DISTANCES: [(f64, fn(&ArmorPowerSeries) -> Option<f64>); 12] = [
	(0.0, |s| s.ap_0m),
	(100.0, |s| s.ap_100m),
	(500.0, |s| s.ap_500m),
	(1000.0, |s| s.ap_1000m),
	(1500.0, |s| s.ap_1500m),
	(2000.0, |s| s.ap_2000m),
	(2500.0, |s| s.ap_2500m),
	(3000.0, |s| s.ap_3000m),
	(3500.0, |s| s.ap_3500m),
	(4000.0, |s| s.ap_4000m),
	(4500.0, |s| s.ap_4500m),
	(10000.0, |s| s.ap_10000m),
];

/// Convert a [`Projectile`] (Stage 1 model) directly into a [`DataProjectile`]
/// (Stage 2 model) without going through the text roundtrip.
///
/// This bridges the in-memory pipeline: `convert_vehicle` produces
/// `VehicleData` with `Projectile` entries; this function converts each
/// into the flat `DataProjectile` that [`compute_ballistic`] expects.
///
/// The conversion faithfully reproduces the same defaulting and
/// transformation logic that would occur if the data were serialised
/// via `emit_legacy_txt` and re-parsed via `parse_data_text`.
///
/// [`compute_ballistic`]: crate::ballistic::compute_ballistic
#[must_use]
pub fn from_projectile(proj: &Projectile) -> DataProjectile {
	let normalized_type = normalize_shell_type(&proj.bullet_type);
	let output_name = clean_shell_name(&proj.name);

	// Flatten DeMarre parameters (0.0 when absent — ballistic.rs applies
	// its own non-zero defaults at compute time).
	let (demarre_k, demarre_speed_pow, demarre_mass_pow, demarre_caliber_pow) =
		proj.demarre.as_ref().map_or(
			(0.0, 0.0, 0.0, 0.0),
			|d| (d.k, d.speed_pow, d.mass_pow, d.caliber_pow),
		);

	// Build APDS armor power table from the named series fields.
	let armor_power_table = proj
		.armor_power_series
		.as_ref()
		.map_or_else(Vec::new, |series| {
			ARMOR_POWER_DISTANCES
				.iter()
				.filter_map(|&(dist, getter)| getter(series).map(|pen| (dist, pen)))
				.collect()
		});

	DataProjectile {
		name: proj.name.clone(),
		bullet_type: proj.bullet_type.clone(),
		normalized_type,
		mass: proj.mass.unwrap_or(0.0),
		ballistic_caliber: proj.ballistic_caliber.unwrap_or(0.0),
		speed: proj.speed.unwrap_or(0.0),
		cx: proj.cx.unwrap_or(DEFAULT_CX),
		explosive_mass: proj.explosive_mass.unwrap_or(0.0),
		damage_mass: proj.damage_mass.unwrap_or(0.0),
		damage_caliber: proj.damage_caliber.unwrap_or(0.0),
		demarre_k,
		demarre_speed_pow,
		demarre_mass_pow,
		demarre_caliber_pow,
		armor_power_table,
		output_name,
	}
}

#[cfg(test)]
mod tests {
	use super::*;

	#[test]
	fn test_normalize_shell_type() {
		assert_eq!(normalize_shell_type("apcbc_tank"), "apcbc");
		assert_eq!(normalize_shell_type("he_frag_tank"), "he");
		assert_eq!(normalize_shell_type("heat_fs_tank"), "heat");
		assert_eq!(normalize_shell_type("apds_fs_long_tank"), "apds_fs");
		assert_eq!(normalize_shell_type("apds_fs_tungsten_l10_l15_tank"), "apds_fs");
		assert_eq!(normalize_shell_type("apcr_tank"), "apcr");
		assert_eq!(normalize_shell_type("apds_autocannon"), "apds");
		assert_eq!(normalize_shell_type("atgm_tandem_tank"), "atgm");
		assert_eq!(normalize_shell_type("smoke_tank"), "smoke");
	}

	#[test]
	fn test_clean_shell_name() {
		assert_eq!(clean_shell_name("75mm_pzgr_39"), "pzgr_39");
		assert_eq!(clean_shell_name("105mm_m735"), "m735");
		assert_eq!(clean_shell_name("120mm_m829a2"), "m829a2");
		assert_eq!(clean_shell_name("30mm_UBR6"), "UBR6");
		assert_eq!(clean_shell_name("some_bullet"), "some_bullet");
		assert_eq!(clean_shell_name("105mm_m735/something"), "m735");
	}

	#[test]
	fn test_parse_data_text() {
		let content = "\
WeaponPath:gameData/Weapons/test.blkx
ZoomIn:9.21
ZoomOut:28.63
HasLaser

Name:105mm_m735
Type:apds_fs_tungsten_l10_l15_tank
BulletMass:3.719457
BallisticCaliber:0.035
Speed:1501.14
Cx:0.2925
DamageCaliber:0.03175
APDS0:292.4
APDS100:290.6
APDS500:284.0
APDS1000:275.0
APDS1500:265.9
APDS2000:256.5
APDS2500:246.7
APDS3000:236.7
APDS4000:215.5
APDS10000:50.0

Name:75mm_pzgr_39
Type:apcbc_tank
BulletMass:6.8
BallisticCaliber:0.075
Speed:740.0
Cx:0.4
ExplosiveMass:0.017
ExplosiveType:h10
demarrePenetrationK:1.0
demarreSpeedPow:1.43
demarreMassPow:0.71
demarreCaliberPow:1.07";

		let data = parse_data_text(content, "test_vehicle");

		assert_eq!(data.vehicle_id, "test_vehicle");
		assert_eq!(data.weapon_path.as_deref(), Some("gameData/Weapons/test.blkx"));
		assert!(data.has_laser);
		assert_eq!(data.zoom_in, Some(9.21));
		assert_eq!(data.projectiles.len(), 2);

		let m735 = &data.projectiles[0];
		assert_eq!(m735.output_name, "m735");
		assert_eq!(m735.normalized_type, "apds_fs");
		assert_eq!(m735.armor_power_table.len(), 10);
		assert!((m735.armor_power_table[0].0 - 0.0).abs() < f64::EPSILON);
		assert!((m735.armor_power_table[0].1 - 292.4).abs() < f64::EPSILON);

		let pzgr = &data.projectiles[1];
		assert_eq!(pzgr.output_name, "pzgr_39");
		assert_eq!(pzgr.normalized_type, "apcbc");
		assert!((pzgr.demarre_k - 1.0).abs() < f64::EPSILON);
		assert!((pzgr.explosive_mass - 0.017).abs() < f64::EPSILON);
	}
}
