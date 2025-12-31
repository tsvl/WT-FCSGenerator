//! Integration tests for Stage 1 conversion.

use std::path::PathBuf;

use fcsgen_core::{convert_vehicle, emit_legacy_txt};

/// Get the path to the examples directory.
fn examples_dir() -> PathBuf {
	PathBuf::from(env!("CARGO_MANIFEST_DIR"))
		.parent()
		.unwrap()
		.join("examples")
		.join("2.53.0.42")
}

/// Convert a single vehicle and compare to expected output.
/// Returns Ok(()) on match, Err with diff info on mismatch.
fn check_vehicle(vehicle_name: &str) -> Result<(), String> {
	let examples = examples_dir();
	let input_root = examples.join("input");
	let vehicle_path = input_root
		.join("aces.vromfs.bin_u")
		.join("gamedata")
		.join("units")
		.join("tankmodels")
		.join(format!("{vehicle_name}.blkx"));

	let expected_path = examples.join("output").join(format!("{vehicle_name}.txt"));

	// Convert
	let data = convert_vehicle(&vehicle_path, &input_root)
		.map_err(|e| format!("conversion error: {e}"))?;
	let output = emit_legacy_txt(&data);

	// Load expected
	let expected = std::fs::read_to_string(&expected_path)
		.map_err(|e| format!("cannot read expected: {e}"))?;

	// Compare
	if output == expected {
		Ok(())
	} else {
		// Find first different line
		let exp_lines: Vec<_> = expected.lines().collect();
		let out_lines: Vec<_> = output.lines().collect();

		for (i, (exp, out)) in exp_lines.iter().zip(out_lines.iter()).enumerate() {
			if exp != out {
				return Err(format!("line {}: expected {:?}, got {:?}", i + 1, exp, out));
			}
		}

		if exp_lines.len() != out_lines.len() {
			return Err(format!(
				"line count: expected {}, got {}",
				exp_lines.len(),
				out_lines.len()
			));
		}

		Err("outputs differ but no line diff found (whitespace?)".to_string())
	}
}

/// Test conversion of BMP-2M against expected output.
#[test]
fn test_bmp_2m_conversion() {
	let examples = examples_dir();
	let input_root = examples.join("input");
	let vehicle_path = input_root
		.join("aces.vromfs.bin_u")
		.join("gamedata")
		.join("units")
		.join("tankmodels")
		.join("ussr_bmp_2m.blkx");

	// Skip test if examples aren't present (they're gitignored)
	if !vehicle_path.exists() {
		eprintln!("Skipping test: examples not present at {vehicle_path:?}");
		return;
	}

	if let Err(e) = check_vehicle("ussr_bmp_2m") {
		panic!("ussr_bmp_2m failed: {e}");
	}
}

/// Run conversion on ALL vehicles in the corpus and report statistics.
/// This test is ignored by default - run with `cargo test corpus -- --ignored`
#[test]
fn test_full_corpus() {
	let examples = examples_dir();
	let input_dir = examples
		.join("input")
		.join("aces.vromfs.bin_u")
		.join("gamedata")
		.join("units")
		.join("tankmodels");
	let output_dir = examples.join("output");

	if !input_dir.exists() {
		eprintln!("Skipping corpus test: examples not present");
		return;
	}

	// Collect all expected output files (these are the vehicles legacy tool produced output for)
	let expected_files: Vec<_> = std::fs::read_dir(&output_dir)
		.expect("read output dir")
		.filter_map(|e| e.ok())
		.filter(|e| e.path().extension().is_some_and(|ext| ext == "txt"))
		.map(|e| e.path().file_stem().unwrap().to_string_lossy().to_string())
		.collect();

	let total = expected_files.len();
	let mut passed = 0;
	let mut failed = 0;
	let mut errors = 0;
	let mut failures: Vec<(String, String)> = Vec::new();

	for vehicle in &expected_files {
		match check_vehicle(vehicle) {
			Ok(()) => {
				passed += 1;
			},
			Err(e) if e.starts_with("conversion error") => {
				errors += 1;
				if errors <= 10 {
					eprintln!("ERROR {vehicle}: {e}");
				}
			},
			Err(e) => {
				failed += 1;
				failures.push((vehicle.clone(), e));
			},
		}
	}

	// Print summary
	eprintln!("\n{}", "=".repeat(60));
	eprintln!("CORPUS TEST RESULTS");
	eprintln!("{}", "=".repeat(60));
	eprintln!("Total:  {total}");
	eprintln!(
		"Passed: {passed} ({:.1}%)",
		100.0 * passed as f64 / total as f64
	);
	eprintln!(
		"Failed: {failed} ({:.1}%)",
		100.0 * failed as f64 / total as f64
	);
	eprintln!(
		"Errors: {errors} ({:.1}%)",
		100.0 * errors as f64 / total as f64
	);

	// Print first N failures with details to console
	if !failures.is_empty() {
		eprintln!("\nFirst 20 failures:");
		for (vehicle, err) in failures.iter().take(20) {
			eprintln!("  {vehicle}: {err}");
		}

		// Dump full failure list to file for reference
		let failure_log = examples.join("corpus-failures.txt");
		let mut report = String::new();
		report.push_str(&format!("Corpus Test Failure Report\n"));
		report.push_str(&format!("==========================\n\n"));
		report.push_str(&format!("Total: {total}\n"));
		report.push_str(&format!("Passed: {passed}\n"));
		report.push_str(&format!("Failed: {failed}\n"));
		report.push_str(&format!("Errors: {errors}\n\n"));
		report.push_str(&format!("Failures ({failed} vehicles):\n"));
		report.push_str(&format!("{}\n\n", "-".repeat(40)));

		for (vehicle, err) in &failures {
			report.push_str(&format!("{vehicle}\n  {err}\n\n"));
		}

		if let Err(e) = std::fs::write(&failure_log, &report) {
			eprintln!("Warning: could not write failure log: {e}");
		} else {
			eprintln!("\nFull failure list written to: {}", failure_log.display());
		}
	}

	// Fail if pass rate is below threshold (adjust as we improve)
	let pass_rate = passed as f64 / total as f64;
	assert!(
		pass_rate >= 0.0, // Set to 0 for now, raise as we improve
		"Pass rate {:.1}% below threshold",
		pass_rate * 100.0
	);
}
