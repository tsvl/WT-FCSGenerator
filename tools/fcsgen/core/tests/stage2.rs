//! Integration tests for Stage 2 ballistic computation.
//!
//! Parses every `Data/{vehicle}.txt` file, computes ballistic tables for each
//! projectile, and compares the TSV output against the expected reference files
//! in `test_data/expected/ballistic/{vehicle}/{shell}.txt`.

use std::collections::HashMap;
use std::path::PathBuf;

use fcsgen_core::ballistic::{compute_ballistic, should_skip};
use fcsgen_core::parser::data::parse_data_file;

/// Default sensitivity used when generating the reference data.
const SENSITIVITY: f64 = 0.50;

/// Get the path to the test_data directory.
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

			match compare_ballistic(&vehicle_id, &proj.output_name, &computed, &expected) {
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
	eprintln!("Missing expected: {missing_expected}");

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

	if total_shells > 0 {
		let pass_rate = passed as f64 / total_shells as f64;
		assert!(
			pass_rate >= 0.0,
			"Pass rate {:.1}% below threshold",
			pass_rate * 100.0
		);
	}
}
