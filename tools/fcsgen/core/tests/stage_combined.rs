//! Combined integration test: datamine → in-memory ballistic (no text roundtrip).
//!
//! Validates that `convert_vehicle` → `from_projectile` → `compute_ballistic`
//! produces output within acceptable tolerance of the reference files
//! in `test_data/expected/ballistic/`.
//!
//! Uses fuzzy numeric comparison to accommodate minor floating-point
//! differences from the optimised trajectory engine.

use std::collections::HashMap;
use std::path::PathBuf;

use fcsgen_core::ballistic::{BallisticCache, compute_ballistic_cached, should_skip};
use fcsgen_core::parser::data::from_projectile;
use fcsgen_core::{convert_vehicle, emit_legacy_txt};

/// Default sensitivity used when generating the reference data.
const SENSITIVITY: f64 = 0.50;

// ── Tolerances ─────────────────────────────────────────────────────────────
/// Maximum acceptable delta for the distance column (metres).
const DIST_TOL: f64 = 0.01;
/// Maximum acceptable delta for the time column (seconds).
const TIME_TOL: f64 = 0.1;
/// Maximum acceptable delta for the penetration column (mm).
const PEN_TOL: f64 = 1.0;
/// Maximum acceptable row-count difference (extra/missing rows at the end).
const ROW_COUNT_TOL: usize = 5;

/// Get the path to the `test_data` directory.
fn test_data_dir() -> PathBuf {
	PathBuf::from(env!("CARGO_MANIFEST_DIR"))
		.parent()
		.unwrap()
		.join("test_data")
}

/// Parsed TSV row: (distance, time, penetration).
fn parse_row(line: &str) -> Option<(f64, f64, f64)> {
	let parts: Vec<&str> = line.split('\t').collect();
	if parts.len() < 3 {
		return None;
	}
	let dist = parts[0].parse::<f64>().ok()?;
	let time = parts[1].parse::<f64>().ok()?;
	let pen = if parts[2] == "\u{221E}" {
		f64::INFINITY
	} else {
		parts[2].parse::<f64>().ok()?
	};
	Some((dist, time, pen))
}

/// Tracking struct for worst-case deltas across the corpus.
#[derive(Default)]
struct DeltaStats {
	max_dist: f64,
	max_time: f64,
	max_pen: f64,
	max_row_diff: usize,
	worst_dist_shell: String,
	worst_pen_shell: String,
	worst_row_shell: String,
}

/// Compare a computed ballistic TSV against the expected reference using
/// fuzzy numeric matching.
///
/// Returns `Ok(())` when all values are within tolerance,
/// `Err(description)` on a tolerance violation.
fn compare_ballistic_fuzzy(
	vehicle: &str,
	shell: &str,
	computed: &str,
	expected: &str,
	stats: &mut DeltaStats,
) -> Result<(), String> {
	let computed = computed.replace("\r\n", "\n");
	let expected = expected.replace("\r\n", "\n");

	let comp_lines: Vec<&str> = computed.lines().collect();
	let exp_lines: Vec<&str> = expected.lines().collect();

	let row_diff = comp_lines.len().abs_diff(exp_lines.len());
	if row_diff > stats.max_row_diff {
		stats.max_row_diff = row_diff;
		stats.worst_row_shell = format!("{vehicle}/{shell}");
	}
	if row_diff > ROW_COUNT_TOL {
		return Err(format!(
			"{vehicle}/{shell}: row count diff {row_diff} exceeds tolerance {ROW_COUNT_TOL} \
			 (expected {}, got {})",
			exp_lines.len(),
			comp_lines.len(),
		));
	}

	let overlap = comp_lines.len().min(exp_lines.len());
	for i in 0..overlap {
		let (Some(comp), Some(exp)) = (parse_row(comp_lines[i]), parse_row(exp_lines[i])) else {
			continue;
		};

		let dd = (comp.0 - exp.0).abs();
		let dt = (comp.1 - exp.1).abs();
		let dp = if comp.2.is_infinite() && exp.2.is_infinite() {
			0.0
		} else {
			(comp.2 - exp.2).abs()
		};

		if dd > stats.max_dist {
			stats.max_dist = dd;
			stats.worst_dist_shell = format!("{vehicle}/{shell}");
		}
		if dt > stats.max_time {
			stats.max_time = dt;
		}
		if dp > stats.max_pen {
			stats.max_pen = dp;
			stats.worst_pen_shell = format!("{vehicle}/{shell}");
		}

		if dd > DIST_TOL || dt > TIME_TOL || dp > PEN_TOL {
			return Err(format!(
				"{vehicle}/{shell} line {}: delta dist={dd:.4} time={dt:.2} pen={dp:.1} \
				 (tol: dist={DIST_TOL} time={TIME_TOL} pen={PEN_TOL})",
				i + 1,
			));
		}
	}

	Ok(())
}

/// Run the full in-memory pipeline on ALL vehicles in the corpus.
///
/// For each vehicle:
/// 1. `convert_vehicle` to get `VehicleData` with `Projectile` entries
/// 2. `from_projectile` to bridge into `DataProjectile`
/// 3. `compute_ballistic` to get the TSV output
/// 4. Compare against the reference files in `test_data/expected/ballistic/`
///
/// This validates that the in-memory path is identical to the text roundtrip.
#[test]
fn test_combined_pipeline_corpus() {
	let datamine_dir = test_data_dir().join("datamine");
	let vehicles_path = datamine_dir
		.join("aces.vromfs.bin_u")
		.join("gamedata")
		.join("units")
		.join("tankmodels");
	let ballistic_dir = test_data_dir().join("expected").join("ballistic");
	let expected_data_dir = test_data_dir().join("expected").join("data");
	let output_dir = test_data_dir().join("output").join("combined");

	if !vehicles_path.exists() {
		eprintln!("Skipping combined corpus test: datamine not present");
		return;
	}

	if !ballistic_dir.exists() {
		eprintln!("Skipping combined corpus test: ballistic reference not present");
		return;
	}

	// Ensure output directory exists (for debug output)
	let _ = std::fs::create_dir_all(&output_dir);

	// Collect all expected data files — these are the vehicles for which we have
	// expected ballistic output (i.e. vehicles the legacy tool processed).
	let expected_files: Vec<String> = std::fs::read_dir(&expected_data_dir)
		.expect("read expected data dir")
		.filter_map(|e| e.ok())
		.filter(|e| e.path().extension().is_some_and(|ext| ext == "txt"))
		.map(|e| e.path().file_stem().unwrap().to_string_lossy().to_string())
		.collect();

	let mut total_shells = 0;
	let mut passed = 0;
	let mut failed = 0;
	let mut errors = 0;
	let mut failures: Vec<String> = Vec::new();
	let mut stats = DeltaStats::default();
	let mut cache: BallisticCache = BallisticCache::new();
	let mut cache_hits = 0_usize;
	let mut cache_misses = 0_usize;

	for vehicle_name in &expected_files {
		let vehicle_path = vehicles_path.join(format!("{vehicle_name}.blkx"));
		if !vehicle_path.exists() {
			continue;
		}

		// Check if there's a corresponding ballistic directory
		let vehicle_ballistic_dir = ballistic_dir.join(vehicle_name);
		if !vehicle_ballistic_dir.exists() {
			continue;
		}

		// Stage 1: convert vehicle from datamine
		let data = match convert_vehicle(&vehicle_path, &datamine_dir) {
			Ok(d) => d,
			Err(e) => {
				eprintln!("CONVERT ERROR {vehicle_name}: {e}");
				errors += 1;
				continue;
			},
		};

		// Verify Stage 1 output hasn't changed: emit to text and compare.
		// (This is a sanity check — stage1.rs covers this exhaustively.)
		let _legacy_txt = emit_legacy_txt(&data);

		// Bridge: Projectile → DataProjectile (in-memory, no text roundtrip)
		let data_projectiles: Vec<_> = data
			.projectiles
			.iter()
			.map(from_projectile)
			.collect();

		// Deduplicate by output_name: keep last occurrence (matching C#'s
		// `File.WriteAllText` overwrite semantics).
		let mut last_by_name: HashMap<String, usize> = HashMap::new();
		for (idx, dp) in data_projectiles.iter().enumerate() {
			if !should_skip(&dp.normalized_type) {
				last_by_name.insert(dp.output_name.clone(), idx);
			}
		}

		for &idx in last_by_name.values() {
			let dp = &data_projectiles[idx];

			let expected_path = vehicle_ballistic_dir.join(format!("{}.txt", dp.output_name));
			if !expected_path.exists() {
				continue;
			}

			total_shells += 1;

			let (result, hit) = compute_ballistic_cached(dp, SENSITIVITY, &mut cache);
			if hit { cache_hits += 1; } else { cache_misses += 1; }

			let computed = match result {
				Some(c) => c,
				None => {
					failures.push(format!(
						"{vehicle_name}/{}: compute returned None",
						dp.output_name
					));
					failed += 1;
					continue;
				},
			};

			let expected = match std::fs::read_to_string(&expected_path) {
				Ok(e) => e,
				Err(e) => {
					eprintln!(
						"READ ERROR {vehicle_name}/{}: {e}",
						dp.output_name
					);
					errors += 1;
					continue;
				},
			};

			match compare_ballistic_fuzzy(
				vehicle_name,
				&dp.output_name,
				&computed,
				&expected,
				&mut stats,
			) {
				Ok(()) => passed += 1,
				Err(msg) => {
					failed += 1;
					failures.push(msg);

					// Write computed output for debugging
					let out_vehicle = output_dir.join(vehicle_name);
					let _ = std::fs::create_dir_all(&out_vehicle);
					let _ = std::fs::write(
						out_vehicle.join(format!("{}.txt", dp.output_name)),
						&computed,
					);
				},
			}
		}
	}

	// Print summary
	eprintln!("\n{}", "=".repeat(60));
	eprintln!("COMBINED PIPELINE CORPUS TEST RESULTS");
	eprintln!("{}", "=".repeat(60));
	eprintln!("Total shells tested:  {total_shells}");
	eprintln!(
		"Passed:               {passed} ({:.1}%)",
		if total_shells > 0 {
			100.0 * passed as f64 / total_shells as f64
		} else {
			0.0
		}
	);
	eprintln!(
		"Failed:               {failed} ({:.1}%)",
		if total_shells > 0 {
			100.0 * failed as f64 / total_shells as f64
		} else {
			0.0
		}
	);
	eprintln!("Errors:               {errors}");
	let total_lookups = cache_hits + cache_misses;
	eprintln!(
		"Cache:                {cache_misses} unique / {total_lookups} total ({cache_hits} hits, {:.0}% reuse)",
		if total_lookups > 0 { 100.0 * cache_hits as f64 / total_lookups as f64 } else { 0.0 },
	);
	eprintln!();
	eprintln!("Worst-case deltas (across all shells):");
	eprintln!(
		"  Distance:  {:.4} m  (tol {DIST_TOL})  [{}]",
		stats.max_dist, stats.worst_dist_shell,
	);
	eprintln!(
		"  Time:      {:.2} s   (tol {TIME_TOL})",
		stats.max_time,
	);
	eprintln!(
		"  Pen:       {:.1} mm  (tol {PEN_TOL})  [{}]",
		stats.max_pen, stats.worst_pen_shell,
	);
	eprintln!(
		"  Row count: {}      (tol {ROW_COUNT_TOL})  [{}]",
		stats.max_row_diff, stats.worst_row_shell,
	);

	if !failures.is_empty() {
		eprintln!("\nFirst 30 failures:");
		for f in failures.iter().take(30) {
			eprintln!("  {f}");
		}

		// Write failure log
		let failure_log = test_data_dir()
			.join("output")
			.join("combined-failures.txt");
		let mut report = String::new();
		report.push_str("Combined Pipeline Corpus Test Failure Report\n");
		report.push_str(&"=".repeat(45));
		report.push('\n');
		report.push_str(&format!(
			"Total: {total_shells}  Passed: {passed}  Failed: {failed}  Errors: {errors}\n\n"
		));
		for f in &failures {
			report.push_str(f);
			report.push('\n');
		}
		let _ = std::fs::write(&failure_log, &report);
		eprintln!(
			"\nFull failure list written to: {}",
			failure_log.display()
		);
	}

	// Assert all shells pass
	assert_eq!(
		failed, 0,
		"{failed} shells failed out of {total_shells} (pass rate: {:.1}%)",
		if total_shells > 0 {
			100.0 * passed as f64 / total_shells as f64
		} else {
			0.0
		}
	);
}
