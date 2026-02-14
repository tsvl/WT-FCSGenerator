//! Ballistic trajectory computation and penetration calculation.
//!
//! Implements the Euler-method trajectory simulation and `DeMarre` penetration
//! formula, matching the C# `Ballistic()` method in Form1.cs.

use std::f64::consts::PI;
use std::fmt::Write;

use crate::parser::data::DataProjectile;

// ── Physics constants ──────────────────────────────────────────────────────
const G: f64 = 9.806_65;
const DT: f64 = 0.01;
const P_ATM: f64 = 101_325.0;
const T_GROUND: f64 = 15.0;
const M_AIR: f64 = 0.028_965_2;
const R_GAS: f64 = 8.314_46;
const LAPSE_RATE: f64 = 0.0065;
const T_STD: f64 = 288.15;
const DEMARRE_REF_V: f64 = 1900.0;
const MAX_RANGE: f64 = 4500.0;

// ── DeMarre defaults (applied when the parsed value is zero) ───────────────
const DEFAULT_K: f64 = 0.9;
const DEFAULT_SPEED_POW: f64 = 1.43;
const DEFAULT_MASS_POW: f64 = 0.71;
const DEFAULT_CALIBER_POW: f64 = 1.07;

// ── Shell type classification ──────────────────────────────────────────────
const AP_TYPES: &[&str] = &[
	"i", "t", "ac", "aphe", "aphebc", "ap", "sap", "sapi", "apc", "apbc",
	"apcbc", "sapcbc",
];
const APHE_TYPES: &[&str] = &["aphe", "aphebc", "ac", "sapcbc", "sap", "sapi"];
const SKIP_TYPES: &[&str] = &["sam", "atgm", "rocket", "aam"];

// ── APHE explosive-filler penalty table (ratio threshold → multiplier) ─────
const PEN_BY_EXPL: [(f64, f64); 5] = [
	(0.0065, 1.0),
	(0.016, 0.93),
	(0.02, 0.9),
	(0.03, 0.85),
	(0.04, 0.75),
];

// ── APCR/APDS subcaliber mass-ratio table ──────────────────────────────────
const PEN_BY_SUBCALIBER: [(f64, f64); 4] = [
	(0.0, 0.25),
	(0.15, 0.4),
	(0.3, 0.5),
	(0.4, 0.75),
];

/// Intermediate row produced by the trajectory simulation.
struct Row {
	distance: f64,
	time: f64,
	penetration: f64,
}

/// Returns `true` if this shell type should be skipped entirely.
#[must_use]
pub fn should_skip(normalized_type: &str) -> bool {
	SKIP_TYPES.contains(&normalized_type)
}

/// Compute the ballistic table for a single projectile.
///
/// Returns the TSV-formatted output string (`distance\ttime\tpenetration\n`
/// per line), or `None` if the projectile type is skipped.
#[must_use]
#[allow(clippy::cast_possible_truncation, clippy::cast_sign_loss, clippy::cast_precision_loss)]
pub fn compute_ballistic(proj: &DataProjectile, sensitivity: f64) -> Option<String> {
	if should_skip(&proj.normalized_type) || sensitivity <= 0.0 {
		return None;
	}

	// DeMarre parameters with defaults applied
	let k = non_zero_or(proj.demarre_k, DEFAULT_K);
	let speed_pow = non_zero_or(proj.demarre_speed_pow, DEFAULT_SPEED_POW);
	let mass_pow = non_zero_or(proj.demarre_mass_pow, DEFAULT_MASS_POW);
	let caliber_pow = non_zero_or(proj.demarre_caliber_pow, DEFAULT_CALIBER_POW);

	let scroll_step = 2.8 * sensitivity * sensitivity;
	let max_entries = (PI / 180.0 * 60.0 * 1000.0 / scroll_step).floor() as usize;

	// Pre-computed barometric exponent (constant for a given atmosphere).
	let baro_exp = G * M_AIR / R_GAS / LAPSE_RATE - 1.0;
	let temp_kelvin = T_GROUND + 273.15;

	let ntype = proj.normalized_type.as_str();
	let is_ap = AP_TYPES.contains(&ntype);
	let is_aphe = APHE_TYPES.contains(&ntype);
	let is_subcaliber = ntype == "apcr" || ntype == "apds";
	let is_apds_fs = ntype == "apds_fs";

	let mut rows: Vec<Row> = Vec::with_capacity(max_entries.min(512));
	let mut last_distance = 0.0_f64;

	for i in 0..max_entries {
		if last_distance >= MAX_RANGE {
			break;
		}

		let angle = scroll_step * (i as f64) / 1000.0;
		let mut vx = proj.speed * angle.cos();
		let mut vy = proj.speed * angle.sin();
		let (mut x, mut y, mut t) = (0.0_f64, 0.0_f64, 0.0_f64);
		let (mut x0, mut y0) = (0.0_f64, 0.0_f64);

		while y >= 0.0 {
			let ro = P_ATM * M_AIR / R_GAS / temp_kelvin
				* (1.0 - LAPSE_RATE * y / T_STD).powf(baro_exp);

			let v_sq = vx * vx + vy * vy;
			let drag =
				proj.cx * ro * v_sq / 2.0 * proj.ballistic_caliber.powi(2) / 4.0 * PI;
			let accel = drag / proj.mass;

			// NOTE: vx is updated *first*; the vy update sees the new vx when
			// evaluating atan(vy/vx).  This matches the C# evaluation order.
			let a1 = (vy / vx).atan();
			vx += (-accel * a1.cos()) * DT;

			let a2 = (vy / vx).atan(); // uses updated vx, original vy
			vy += (-G - accel * a2.sin()) * DT;

			t += DT;
			x0 = x;
			y0 = y;
			x += vx * DT;
			y += vy * DT;
		}

		// Interpolate the ground-crossing distance.
		let distance = x0 + (x - x0) / (y - y0) * (-y0);
		last_distance = distance;

		let time = (t * 10.0).round() / 10.0; // 1-decimal, away-from-zero
		let v_impact = (vx * vx + vy * vy).sqrt();

		let penetration = if is_ap {
			let mut pen = k
				* (v_impact / DEMARRE_REF_V).powf(speed_pow)
				* proj.mass.powf(mass_pow)
				/ (proj.ballistic_caliber * 10.0).powf(caliber_pow)
				* 100.0;

			if is_aphe {
				pen *= aphe_penalty(proj.explosive_mass / proj.mass);
			}
			pen.round()
		} else if is_subcaliber {
			let ratio = proj.damage_mass / proj.mass;
			let sub_k = interpolate_table(&PEN_BY_SUBCALIBER, ratio);
			let effective_mass =
				(proj.mass - proj.damage_mass) * sub_k + proj.damage_mass;

			(k * (v_impact / DEMARRE_REF_V).powf(speed_pow)
				* effective_mass.powf(mass_pow)
				/ (proj.damage_caliber * 10.0).powf(caliber_pow)
				* 100.0)
				.round()
		} else if is_apds_fs {
			interpolate_armor_power(&proj.armor_power_table, distance).round()
		} else {
			0.0
		};

		rows.push(Row {
			distance,
			time,
			penetration,
		});
	}

	// Emit TSV.  Output every row except the last, stopping early on a
	// distance decrease (monotonicity guard, matches C# output loop).
	let mut out = String::new();
	if rows.len() >= 2 {
		for i in 0..rows.len() - 1 {
			if rows[i + 1].distance < rows[i].distance {
				break;
			}
			writeln!(
				out,
				"{:.3}\t{}\t{}",
				rows[i].distance,
				fmt_time(rows[i].time),
				fmt_penetration(rows[i].penetration),
			)
			.unwrap();
		}
	}

	Some(out)
}

// ── Helpers ────────────────────────────────────────────────────────────────

/// APHE explosive-filler penalty factor.
fn aphe_penalty(k: f64) -> f64 {
	interpolate_table(&PEN_BY_EXPL, k)
}

/// Piecewise-linear table lookup matching the C# pattern.
///
/// - Below the first threshold → returns the first value.
/// - Above the last threshold → returns the last value.
/// - Between thresholds → linearly interpolates.
fn interpolate_table(table: &[(f64, f64)], k: f64) -> f64 {
	if k < table[0].0 {
		return table[0].1;
	}
	for window in table.windows(2) {
		let (x0, y0) = window[0];
		let (x1, y1) = window[1];
		if k >= x0 && k < x1 {
			return y0 + (y1 - y0) / (x1 - x0) * (k - x0);
		}
	}
	table[table.len() - 1].1
}

/// Linear interpolation of the APDS-FS armor-power table.
///
/// Returns 0 when the distance falls outside all intervals (this includes
/// the case where the table is empty — i.e. no APDS series was present in
/// the data file).
fn interpolate_armor_power(table: &[(f64, f64)], distance: f64) -> f64 {
	for window in table.windows(2) {
		let (d0, p0) = window[0];
		let (d1, p1) = window[1];
		if distance >= d0 && distance < d1 {
			return p0 + (distance - d0) / (d1 - d0) * (p1 - p0);
		}
	}
	0.0
}

/// Format time for TSV output, matching C# `double.ToString()` behaviour.
///
/// Whole-second values have no decimal point: `"0"`, `"1"`, `"10"`.
/// Fractional values get exactly one decimal: `"0.1"`, `"3.5"`.
#[allow(clippy::cast_possible_truncation)]
fn fmt_time(t: f64) -> String {
	if t.fract().abs() < 1e-9 {
		format!("{}", t as i64)
	} else {
		format!("{t:.1}")
	}
}

/// Format penetration for TSV output, matching C# `double.ToString()`.
///
/// Finite values are integers: `"138"`, `"0"`.
/// Infinite values are the infinity symbol: `"∞"` (matches C# behaviour).
#[allow(clippy::cast_possible_truncation)]
fn fmt_penetration(p: f64) -> String {
	if p.is_infinite() || p.is_nan() {
		"\u{221E}".to_owned() // ∞
	} else {
		format!("{}", p as i64)
	}
}

/// Return `val` when it is non-zero, otherwise `default`.
fn non_zero_or(val: f64, default: f64) -> f64 {
	if val == 0.0 { default } else { val }
}

#[cfg(test)]
mod tests {
	use super::*;

	#[test]
	fn test_interpolate_table_below() {
		assert!((interpolate_table(&PEN_BY_EXPL, 0.001) - 1.0).abs() < f64::EPSILON);
	}

	#[test]
	fn test_interpolate_table_above() {
		assert!((interpolate_table(&PEN_BY_EXPL, 0.05) - 0.75).abs() < f64::EPSILON);
	}

	#[test]
	fn test_interpolate_table_middle() {
		// Between (0.0065, 1.0) and (0.016, 0.93)
		let k = 0.01;
		let expected = 1.0 + (0.93 - 1.0) / (0.016 - 0.0065) * (0.01 - 0.0065);
		assert!((interpolate_table(&PEN_BY_EXPL, k) - expected).abs() < 1e-10);
	}

	#[test]
	fn test_interpolate_subcaliber() {
		// k = 0.0 → 0.25
		assert!((interpolate_table(&PEN_BY_SUBCALIBER, 0.0) - 0.25).abs() < f64::EPSILON);
		// k = 0.4 → 0.75
		assert!((interpolate_table(&PEN_BY_SUBCALIBER, 0.4) - 0.75).abs() < f64::EPSILON);
		// k = 0.5 → 0.75 (above last)
		assert!((interpolate_table(&PEN_BY_SUBCALIBER, 0.5) - 0.75).abs() < f64::EPSILON);
	}

	#[test]
	fn test_fmt_time() {
		assert_eq!(fmt_time(0.0), "0");
		assert_eq!(fmt_time(1.0), "1");
		assert_eq!(fmt_time(10.0), "10");
		assert_eq!(fmt_time(0.1), "0.1");
		assert_eq!(fmt_time(3.5), "3.5");
	}

	#[test]
	fn test_should_skip() {
		assert!(should_skip("atgm"));
		assert!(should_skip("sam"));
		assert!(should_skip("rocket"));
		assert!(should_skip("aam"));
		assert!(!should_skip("apcbc"));
		assert!(!should_skip("he"));
		assert!(!should_skip("apds_fs"));
	}

	#[test]
	fn test_non_zero_or() {
		assert!((non_zero_or(0.0, 0.9) - 0.9).abs() < f64::EPSILON);
		assert!((non_zero_or(1.0, 0.9) - 1.0).abs() < f64::EPSILON);
	}
}
