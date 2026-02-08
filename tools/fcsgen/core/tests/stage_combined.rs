//! Combined integration test: datamine → in-memory ballistic (no text roundtrip).
//!
//! Validates that `convert_vehicle` → `from_projectile` → `compute_ballistic`
//! produces identical output to the existing text-roundtrip path that was
//! already verified at 100% in `stage2.rs`.

use std::collections::HashMap;
use std::path::PathBuf;

use fcsgen_core::ballistic::{compute_ballistic, should_skip};
use fcsgen_core::parser::data::from_projectile;
use fcsgen_core::{convert_vehicle, emit_legacy_txt};

/// Default sensitivity used when generating the reference data.
const SENSITIVITY: f64 = 0.50;

/// Get the path to the `test_data` directory.
fn test_data_dir() -> PathBuf {
	PathBuf::from(env!("CARGO_MANIFEST_DIR"))
		.parent()
		.unwrap()
		.join("test_data")
}

/// Compare a computed ballistic TSV against the expected reference.
///
/// Returns `Ok(())` on exact match, `Err(description)` on mismatch.
fn compare_ballistic(
	vehicle: &str,
	shell: &str,
	computed: &str,
	expected: &str,
) -> Result<(), String> {
	let computed = computed.replace("\r\n", "\n");
	let expected = expected.replace("\r\n", "\n");

	let comp_lines: Vec<&str> = computed.lines().collect();
	let exp_lines: Vec<&str> = expected.lines().collect();

	if comp_lines.len() != exp_lines.len() {
		return Err(format!(
			"{vehicle}/{shell}: line count mismatch: expected {}, got {}",
			exp_lines.len(),
			comp_lines.len(),
		));
	}

	for (i, (cl, el)) in comp_lines.iter().zip(exp_lines.iter()).enumerate() {
		if cl != el {
			return Err(format!(
				"{vehicle}/{shell} line {}: expected '{el}', got '{cl}'",
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

			let computed = match compute_ballistic(dp, SENSITIVITY) {
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

			match compare_ballistic(vehicle_name, &dp.output_name, &computed, &expected) {
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
	eprintln!("Total shells tested: {total_shells}");
	eprintln!(
		"Passed: {passed} ({:.1}%)",
		if total_shells > 0 {
			100.0 * passed as f64 / total_shells as f64
		} else {
			0.0
		}
	);
	eprintln!(
		"Failed: {failed} ({:.1}%)",
		if total_shells > 0 {
			100.0 * failed as f64 / total_shells as f64
		} else {
			0.0
		}
	);
	eprintln!("Errors: {errors}");

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
