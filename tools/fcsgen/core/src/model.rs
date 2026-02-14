//! Data models for FCS generation.
//!
//! These structs represent the intermediate data extracted from datamine files,
//! used for ballistic computation and sight generation.

use serde::{Deserialize, Serialize};

/// Complete vehicle data extracted from datamine, ready for emission.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct VehicleData {
	/// Vehicle identifier (basename of .blkx file, e.g. "ussr_bmp_2m").
	pub id: String,

	/// Path to the primary weapon module (e.g. "gameData/Weapons/groundModels_weapons/...").
	pub weapon_path: Option<String>,

	/// Paths to rocket/ATGM modules (up to 2).
	pub rocket_paths: Vec<String>,

	/// Primary optics zoom (narrow FOV, higher magnification).
	pub zoom_in: Option<f64>,

	/// Primary optics zoom (wide FOV, lower magnification).
	pub zoom_out: Option<f64>,

	/// Secondary optics zoom (narrow FOV), if present.
	pub zoom_in_2: Option<f64>,

	/// Secondary optics zoom (wide FOV), if present.
	pub zoom_out_2: Option<f64>,

	/// Whether the vehicle has a laser rangefinder.
	pub has_laser: bool,

	/// Projectiles from all weapon modules.
	pub projectiles: Vec<Projectile>,
}

/// A single projectile (bullet, shell, or rocket/missile).
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Projectile {
	/// Projectile name (e.g. "30mm_UBR6").
	pub name: String,

	/// Projectile type (e.g. "ap_t", "he_frag_i", "apds_fs_long_tank").
	pub bullet_type: String,

	/// Mass in kg.
	pub mass: Option<f64>,

	/// Ballistic caliber in meters (e.g. 0.03 for 30mm).
	pub ballistic_caliber: Option<f64>,

	/// Muzzle velocity in m/s.
	pub speed: Option<f64>,

	/// Drag coefficient (averaged if source was an array).
	pub cx: Option<f64>,

	/// Explosive filler mass in kg.
	pub explosive_mass: Option<f64>,

	/// Explosive type (e.g. "a_ix_2", "ocfol").
	pub explosive_type: Option<String>,

	/// Damage mass for sub-caliber rounds.
	pub damage_mass: Option<f64>,

	/// Damage caliber for sub-caliber rounds.
	pub damage_caliber: Option<f64>,

	/// DeMarre penetration parameters.
	pub demarre: Option<DemarreParams>,

	/// Armor penetration for ATGMs/rockets (single value).
	pub armor_power: Option<f64>,

	/// Armor power series for APDS/APFSDS (distance -> penetration).
	pub armor_power_series: Option<ArmorPowerSeries>,
}

/// DeMarre penetration formula parameters.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DemarreParams {
	pub k: f64,
	pub speed_pow: f64,
	pub mass_pow: f64,
	pub caliber_pow: f64,
}

/// Distance-indexed armor power values for APDS/APFSDS rounds.
#[derive(Debug, Clone, Default, Serialize, Deserialize)]
pub struct ArmorPowerSeries {
	pub ap_0m: Option<f64>,
	pub ap_100m: Option<f64>,
	pub ap_500m: Option<f64>,
	pub ap_1000m: Option<f64>,
	pub ap_1500m: Option<f64>,
	pub ap_2000m: Option<f64>,
	pub ap_2500m: Option<f64>,
	pub ap_3000m: Option<f64>,
	pub ap_3500m: Option<f64>,
	pub ap_4000m: Option<f64>,
	pub ap_4500m: Option<f64>,
	pub ap_10000m: Option<f64>,
}

impl VehicleData {
	/// Create a new empty vehicle data struct.
	#[must_use]
	pub fn new(id: impl Into<String>) -> Self {
		Self {
			id: id.into(),
			weapon_path: None,
			rocket_paths: Vec::new(),
			zoom_in: None,
			zoom_out: None,
			zoom_in_2: None,
			zoom_out_2: None,
			has_laser: false,
			projectiles: Vec::new(),
		}
	}

	/// Whether the vehicle has any weapon data worth emitting.
	///
	/// Returns `false` for unarmed/non-playable vehicles (e.g. fire control trucks
	/// in multi-part SAM systems) that only have zoom/laser data but no projectiles.
	#[must_use]
	pub fn is_armed(&self) -> bool {
		!self.projectiles.is_empty()
	}
}
