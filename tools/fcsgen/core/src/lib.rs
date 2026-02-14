//! Core library for FCS generation.
//!
//! This crate provides the core functionality for:
//! - Parsing War Thunder datamine files
//! - Computing ballistic trajectories and penetration curves
//! - Generating sight scripts
//!
//! See the CLI crate (`fcsgen`) for the command-line interface.

pub mod ballistic;
pub mod emit;
pub mod error;
pub mod model;
pub mod parser;

pub use ballistic::{BallisticCache, BallisticKey, compute_ballistic, compute_ballistic_cached};
pub use emit::emit_legacy_txt;
pub use error::{ParseError, Result};
pub use model::{Projectile, VehicleData};
pub use parser::data::{from_projectile, parse_data_file, parse_data_text};
pub use parser::{parse_vehicle, parse_weapon_module};

use std::path::Path;

pub const VERSION: &str = env!("CARGO_PKG_VERSION");

/// Convert a vehicle from datamine to legacy Data format.
///
/// This is the main entry point for Stage 1 conversion.
///
/// # Arguments
/// * `vehicle_path` - Path to the vehicle .blkx file
/// * `datamine_root` - Root path of the datamine (contains aces.vromfs.bin_u/)
///
/// # Returns
/// A `VehicleData` struct with all header and projectile data.
pub fn convert_vehicle(vehicle_path: &Path, datamine_root: &Path) -> Result<VehicleData> {
	// Parse vehicle file
	let vehicle_json = read_json_file(vehicle_path)?;
	let vehicle_id = vehicle_path
		.file_stem()
		.and_then(|s| s.to_str())
		.unwrap_or("unknown");

	let mut data = parse_vehicle(&vehicle_json, vehicle_id)?;

	// Parse weapon module and collect projectiles (pass vehicle JSON for belt filtering)
	if let Some(ref weapon_path) = data.weapon_path {
		let full_path = resolve_weapon_path(datamine_root, weapon_path);
		if full_path.exists() {
			let weapon_json = read_json_file(&full_path)?;
			let projectiles = parse_weapon_module(&weapon_json, Some(&vehicle_json))?;
			data.projectiles.extend(projectiles);
		}
	}

	// Parse rocket modules and collect projectiles (pass vehicle JSON for belt filtering)
	for rocket_path in data.rocket_paths.clone() {
		let full_path = resolve_weapon_path(datamine_root, &rocket_path);
		if full_path.exists() {
			let rocket_json = read_json_file(&full_path)?;
			let projectiles = parse_weapon_module(&rocket_json, Some(&vehicle_json))?;
			data.projectiles.extend(projectiles);
		}
	}

	Ok(data)
}

/// Read and parse a JSON file.
fn read_json_file(path: &Path) -> Result<serde_json::Value> {
	let content = std::fs::read_to_string(path).map_err(|e| ParseError::io(path, e))?;
	serde_json::from_str(&content).map_err(|e| ParseError::json(path, e))
}

/// Resolve a weapon path relative to the datamine root.
///
/// Weapon paths in vehicle files look like "gameData/Weapons/..."
/// and need to be resolved relative to the aces.vromfs.bin_u directory.
fn resolve_weapon_path(datamine_root: &Path, weapon_path: &str) -> std::path::PathBuf {
	// Normalize path separators and case for cross-platform compatibility
	let normalized = weapon_path.replace('\\', "/").to_lowercase();
	datamine_root.join("aces.vromfs.bin_u").join(&normalized)
}

#[cfg(test)]
mod tests {
	use super::*;

	#[test]
	fn version_is_set() {
		assert!(!VERSION.is_empty());
	}
}
