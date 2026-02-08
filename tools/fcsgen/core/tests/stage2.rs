//! Integration tests for Stage 2 ballistic computation.
//!
//! Parses every `Data/{vehicle}.txt` file, computes ballistic tables for each
//! projectile, and compares the TSV output against the expected reference files
//! in `test_data/expected/ballistic/{vehicle}/{shell}.txt`.
//!
//! Uses fuzzy numeric comparison to accommodate minor floating-point
//! differences from the optimised trajectory engine (algebraic identity
//! replacements and Taylor-expanded atmospheric density).

use std::collections::HashMap;
use std::path::PathBuf;

use fcsgen_core::ballistic::{compute_ballistic, should_skip};
use fcsgen_core::parser::data::parse_data_file;

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

/// Get the path to the test_data directory.
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
	// Penetration may be "∞"; treat as f64::INFINITY
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

	// Compare the overlapping rows
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
		if dp > stats.max_pen {
			stats.max_pen = dp;
			stats.worst_pen_shell = format!("{vehicle}/{shell}");
		}
		if dt > stats.max_time {
			stats.max_time = dt;
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

/// Run ballistic computation on ALL vehicles in the corpus and report statistics.
#[test]
fn test_ballistic_corpus() {
	let data_dir = test_data_dir().join("expected").join("data");
	let ballistic_dir = test_data_dir().join("expected").join("ballistic");
	let output_dir = test_data_dir().join("output").join("ballistic");

	if !data_dir.exists() {
		eprintln!("Skipping ballistic corpus test: data directory not present");
		return;
	}

	if !ballistic_dir.exists() {
		eprintln!("Skipping ballistic corpus test: ballistic reference not present");
		return;
	}

	// Ensure output directory exists (for debug output)
	let _ = std::fs::create_dir_all(&output_dir);

	// Collect data files
	let mut data_files: Vec<_> = std::fs::read_dir(&data_dir)
		.expect("read data dir")
		.filter_map(|e| e.ok())
		.filter(|e| e.path().extension().is_some_and(|ext| ext == "txt"))
		.collect();

	data_files.sort_by_key(std::fs::DirEntry::file_name);

	let mut total_shells = 0;
	let mut passed = 0;
	let mut failed = 0;
	let mut missing_expected = 0;
	let mut errors = 0;
	let mut failures: Vec<String> = Vec::new();
	let mut stats = DeltaStats::default();

	for entry in &data_files {
		let path = entry.path();
		let vehicle_id = path.file_stem().unwrap().to_string_lossy().to_string();

		// Check if there's a corresponding ballistic directory
		let vehicle_ballistic_dir = ballistic_dir.join(&vehicle_id);
		if !vehicle_ballistic_dir.exists() {
			// No expected ballistic output for this vehicle (e.g. pure SAM/ATGM)
			continue;
		}

		let data = match parse_data_file(&path) {
			Ok(d) => d,
			Err(e) => {
				eprintln!("PARSE ERROR {vehicle_id}: {e}");
				errors += 1;
				continue;
			},
		};

		// Deduplicate projectiles: keep only the last occurrence of each
		// output_name, matching C#'s File.WriteAllText overwrite behaviour.
		let mut last_by_name: HashMap<String, usize> = HashMap::new();
		for (idx, proj) in data.projectiles.iter().enumerate() {
			if !should_skip(&proj.normalized_type) {
				last_by_name.insert(proj.output_name.clone(), idx);
			}
		}

		for &idx in last_by_name.values() {
			let proj = &data.projectiles[idx];

			let expected_path = vehicle_ballistic_dir.join(format!("{}.txt", proj.output_name));
			if !expected_path.exists() {
				missing_expected += 1;
				continue;
			}

			total_shells += 1;

			let computed = match compute_ballistic(proj, SENSITIVITY) {
				Some(c) => c,
				None => {
					failures.push(format!(
						"{vehicle_id}/{}: compute returned None",
						proj.output_name
					));
					failed += 1;
					continue;
				},
			};

			let expected = match std::fs::read_to_string(&expected_path) {
				Ok(e) => e,
				Err(e) => {
					eprintln!(
						"READ ERROR {vehicle_id}/{}: {e}",
						proj.output_name
					);
					errors += 1;
					continue;
				},
			};

			match compare_ballistic_fuzzy(
				&vehicle_id,
				&proj.output_name,
				&computed,
				&expected,
				&mut stats,
			) {
				Ok(()) => passed += 1,
				Err(msg) => {
					failed += 1;
					failures.push(msg);

					// Write computed output for debugging
					let out_vehicle = output_dir.join(&vehicle_id);
					let _ = std::fs::create_dir_all(&out_vehicle);
					let _ = std::fs::write(
						out_vehicle.join(format!("{}.txt", proj.output_name)),
						&computed,
					);
				},
			}
		}
	}

	// Print summary
	eprintln!("\n{}", "=".repeat(60));
	eprintln!("BALLISTIC CORPUS TEST RESULTS");
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
	eprintln!("Missing expected:     {missing_expected}");
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

		// Dump full failure list
		let failure_log = test_data_dir()
			.join("output")
			.join("ballistic-failures.txt");
		let mut report = String::new();
		report.push_str("Ballistic Corpus Test Failure Report\n");
		report.push_str(&"=".repeat(40));
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

	assert_eq!(
		failed, 0,
		"{failed} shells exceeded tolerance out of {total_shells}",
	);
}
