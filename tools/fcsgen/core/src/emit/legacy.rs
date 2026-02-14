//! Legacy Data/{vehicle}.txt format emitter.
//!
//! Produces the exact format expected by the existing WinForms tool's Stage 2 and 3.

use std::fmt::Write;

use crate::model::VehicleData;

/// Format a float value, ensuring it always has a decimal point.
/// E.g., 960 -> "960.0", 960.5 -> "960.5", 0.389 -> "0.389"
fn fmt_float(v: f64) -> String {
	let s = v.to_string();
	if s.contains('.') { s } else { format!("{s}.0") }
}

/// Emit vehicle data in the legacy .txt format.
///
/// The format is:
/// ```text
/// WeaponPath:{path}
/// RocketPath:{path}  (optional, up to 2)
/// ZoomIn:{value}
/// ZoomOut:{value}
/// HasLaser  (optional, presence-only flag)
///
/// Name:{name}
/// Type:{type}
/// BulletMass:{value}
/// ...
/// ```
pub fn emit_legacy_txt(data: &VehicleData) -> String {
	let mut out = String::new();

	// Header
	if let Some(ref wp) = data.weapon_path {
		writeln!(out, "WeaponPath:{wp}").unwrap();
	}

	for rp in &data.rocket_paths {
		writeln!(out, "RocketPath:{rp}").unwrap();
	}

	if let Some(zi) = data.zoom_in {
		writeln!(out, "ZoomIn:{}", fmt_float(zi)).unwrap();
	}

	if let Some(zo) = data.zoom_out {
		writeln!(out, "ZoomOut:{}", fmt_float(zo)).unwrap();
	}

	if data.has_laser {
		writeln!(out, "HasLaser").unwrap();
	}

	// Projectiles
	for proj in &data.projectiles {
		writeln!(out).unwrap(); // Blank line before each projectile block

		writeln!(out, "Name:{}", proj.name).unwrap();
		writeln!(out, "Type:{}", proj.bullet_type).unwrap();

		if let Some(m) = proj.mass {
			writeln!(out, "BulletMass:{}", fmt_float(m)).unwrap();
		}

		if let Some(bc) = proj.ballistic_caliber {
			writeln!(out, "BallisticCaliber:{}", fmt_float(bc)).unwrap();
		}

		if let Some(s) = proj.speed {
			writeln!(out, "Speed:{}", fmt_float(s)).unwrap();
		}

		// Cx with default of 0.38 (legacy behavior for rockets/ATGMs without Cx)
		let cx = proj.cx.unwrap_or(0.38);
		writeln!(out, "Cx:{}", fmt_float(cx)).unwrap();

		if let Some(em) = proj.explosive_mass {
			writeln!(out, "ExplosiveMass:{}", fmt_float(em)).unwrap();
		}

		if let Some(ref et) = proj.explosive_type {
			writeln!(out, "ExplosiveType:{et}").unwrap();
		}

		if let Some(dm) = proj.damage_mass {
			writeln!(out, "DamageMass:{}", fmt_float(dm)).unwrap();
		}

		if let Some(dc) = proj.damage_caliber {
			writeln!(out, "DamageCaliber:{}", fmt_float(dc)).unwrap();
		}

		if let Some(ref demarre) = proj.demarre {
			writeln!(out, "demarrePenetrationK:{}", fmt_float(demarre.k)).unwrap();
			writeln!(out, "demarreSpeedPow:{}", fmt_float(demarre.speed_pow)).unwrap();
			writeln!(out, "demarreMassPow:{}", fmt_float(demarre.mass_pow)).unwrap();
			writeln!(out, "demarreCaliberPow:{}", fmt_float(demarre.caliber_pow)).unwrap();
		}

		if let Some(ap) = proj.armor_power {
			writeln!(out, "ArmorPower:{}", fmt_float(ap)).unwrap();
		}

		// APDS armor power series (legacy field names)
		if let Some(ref series) = proj.armor_power_series {
			if let Some(v) = series.ap_0m {
				writeln!(out, "APDS0:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_100m {
				writeln!(out, "APDS100:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_500m {
				writeln!(out, "APDS500:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_1000m {
				writeln!(out, "APDS1000:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_1500m {
				writeln!(out, "APDS1500:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_2000m {
				writeln!(out, "APDS2000:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_2500m {
				writeln!(out, "APDS2500:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_3000m {
				writeln!(out, "APDS3000:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_3500m {
				writeln!(out, "APDS3500:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_4000m {
				writeln!(out, "APDS4000:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_4500m {
				writeln!(out, "APDS4500:{}", fmt_float(v)).unwrap();
			}
			if let Some(v) = series.ap_10000m {
				writeln!(out, "APDS10000:{}", fmt_float(v)).unwrap();
			}
		}
	}

	// Legacy format doesn't have trailing newline
	out.trim_end().to_string()
}

#[cfg(test)]
mod tests {
	use super::*;
	use crate::model::{DemarreParams, Projectile};

	#[test]
	fn test_emit_basic() {
		let data = VehicleData {
			id: "test_vehicle".to_string(),
			weapon_path: Some("gameData/Weapons/test.blkx".to_string()),
			rocket_paths: vec![],
			zoom_in: Some(6.0),
			zoom_out: Some(30.0),
			has_laser: true,
			projectiles: vec![Projectile {
				name: "test_shell".to_string(),
				bullet_type: "ap_t".to_string(),
				mass: Some(10.0),
				ballistic_caliber: Some(0.1),
				speed: Some(800.0),
				cx: Some(0.3),
				explosive_mass: None,
				explosive_type: None,
				damage_mass: None,
				damage_caliber: None,
				demarre: Some(DemarreParams {
					k: 0.9,
					speed_pow: 1.43,
					mass_pow: 0.71,
					caliber_pow: 1.07,
				}),
				armor_power: None,
				armor_power_series: None,
			}],
		};

		let output = emit_legacy_txt(&data);

		assert!(output.contains("WeaponPath:gameData/Weapons/test.blkx"));
		assert!(output.contains("ZoomIn:6.0"));
		assert!(output.contains("ZoomOut:30.0"));
		assert!(output.contains("HasLaser"));
		assert!(output.contains("Name:test_shell"));
		assert!(output.contains("Type:ap_t"));
		assert!(output.contains("BulletMass:10.0"));
		assert!(output.contains("demarrePenetrationK:0.9"));
	}

	#[test]
	fn test_fmt_float() {
		assert_eq!(fmt_float(960.0), "960.0");
		assert_eq!(fmt_float(960.5), "960.5");
		assert_eq!(fmt_float(0.389), "0.389");
		assert_eq!(fmt_float(1.0), "1.0");
	}
}
