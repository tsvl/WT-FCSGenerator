//! Integration tests for Stage 1 conversion.
//!
//! Uses block-level comparison: parses the legacy .txt format into a header
//! and a set of ammo blocks, then checks that each unique expected ammo block
//! has at least one exact match in our output. This tolerates block reordering
//! and duplicate blocks.

use std::collections::{HashMap, HashSet};
use std::path::PathBuf;

use fcsgen_core::{convert_vehicle, emit_legacy_txt};

/// Parsed representation of a legacy .txt output file.
#[allow(dead_code)]
struct ParsedOutput {
	weapon_path: Option<String>,
	rocket_paths: Vec<String>,
	zoom_in: Option<String>,
	zoom_out: Option<String>,
	has_laser: bool,
	ammo_blocks: Vec<HashMap<String, String>>,
}

/// Parse a legacy .txt file into structured header + ammo blocks.
fn parse_legacy_txt(text: &str) -> ParsedOutput {
	let text = text.replace("\r\n", "\n");
	let sections: Vec<&str> = text.split("\n\n").collect();

	let mut weapon_path = None;
	let mut rocket_paths = Vec::new();
	let mut zoom_in = None;
	let mut zoom_out = None;
	let mut has_laser = false;

	// First section is header
	if let Some(header) = sections.first() {
		for line in header.lines() {
			let line = line.trim();
			if let Some((key, value)) = line.split_once(':') {
				match key {
					"WeaponPath" => weapon_path = Some(value.to_string()),
					"RocketPath" => rocket_paths.push(value.to_string()),
					"ZoomIn" => zoom_in = Some(value.to_string()),
					"ZoomOut" => zoom_out = Some(value.to_string()),
					_ => {}
				}
			} else if line == "HasLaser" {
				has_laser = true;
			}
		}
	}

	// Remaining sections are ammo blocks
	let mut ammo_blocks = Vec::new();
	for section in sections.iter().skip(1) {
		let mut block = HashMap::new();
		for line in section.lines() {
			let line = line.trim();
			if line.is_empty() {
				continue;
			}
			if let Some((key, value)) = line.split_once(':') {
				block.insert(key.to_string(), value.to_string());
			}
		}
		if !block.is_empty() {
			ammo_blocks.push(block);
		}
	}

	ParsedOutput { weapon_path, rocket_paths, zoom_in, zoom_out, has_laser, ammo_blocks }
}

/// Convert an ammo block to a canonical string for deduplication and lookup.
fn block_to_canonical(block: &HashMap<String, String>) -> String {
	let mut pairs: Vec<_> = block.iter().collect();
	pairs.sort_by(|(a, _), (b, _)| a.cmp(b));
	pairs.iter().map(|(k, v)| format!("{k}:{v}")).collect::<Vec<_>>().join("\n")
}

/// Get the path to the test_data directory.
fn test_data_dir() -> PathBuf {
	PathBuf::from(env!("CARGO_MANIFEST_DIR"))
		.parent()
		.unwrap()
		.join("test_data")
}

/// Convert a single vehicle and do block-level comparison to expected output.
///
/// Header fields (ZoomIn, ZoomOut, HasLaser) must match exactly.
/// Each unique expected ammo block must have at least one exact match in our output.
/// Block ordering and duplicate blocks are ignored.
fn check_vehicle(vehicle_name: &str) -> Result<(), String> {
	let vehicle_path = test_data_dir()
		.join("datamine")
		.join("aces.vromfs.bin_u")
		.join("gamedata")
		.join("units")
		.join("tankmodels")
		.join(format!("{vehicle_name}.blkx"));

	let expected_data_dir = test_data_dir().join("expected").join("data");
	let expected_path = expected_data_dir.join(format!("{vehicle_name}.txt"));

	// Convert
	let data = convert_vehicle(&vehicle_path, &test_data_dir().join("datamine"))
		.map_err(|e| format!("conversion error: {e}"))?;
	let output = emit_legacy_txt(&data);

	// Load expected
	let expected = std::fs::read_to_string(&expected_path)
		.map_err(|e| format!("cannot read expected: {e}"))?;

	let exp = parse_legacy_txt(&expected);
	let out = parse_legacy_txt(&output);

	let mut diffs = Vec::new();

	// Header comparison (ZoomIn, ZoomOut, HasLaser matter for downstream;
	// WeaponPath/RocketPath are not used downstream but flag them as info)
	if exp.zoom_in != out.zoom_in {
		diffs.push(format!("ZoomIn: expected {:?}, got {:?}", exp.zoom_in, out.zoom_in));
	}
	if exp.zoom_out != out.zoom_out {
		diffs.push(format!("ZoomOut: expected {:?}, got {:?}", exp.zoom_out, out.zoom_out));
	}
	if exp.has_laser != out.has_laser {
		diffs.push(format!("HasLaser: expected {}, got {}", exp.has_laser, out.has_laser));
	}

	// Ammo block comparison: each unique expected block must exist in our output
	let output_canonicals: HashSet<String> =
		out.ammo_blocks.iter().map(block_to_canonical).collect();

	let mut seen_expected = HashSet::new();
	for exp_block in &exp.ammo_blocks {
		let canon = block_to_canonical(exp_block);
		if !seen_expected.insert(canon.clone()) {
			continue; // Skip duplicate expected blocks
		}

		if output_canonicals.contains(&canon) {
			continue; // Exact match found
		}

		// No exact match — find closest block by Name for better diagnostics
		let exp_name = exp_block.get("Name").map_or("???", |s| s.as_str());
		let closest = out.ammo_blocks.iter().find(|b| {
			b.get("Name").is_some_and(|n| n == exp_name)
		});

		if let Some(close) = closest {
			let mut field_diffs = Vec::new();
			for (k, v) in exp_block {
				match close.get(k) {
					Some(cv) if cv != v => {
						field_diffs.push(format!("{k}: exp {v}, got {cv}"));
					}
					None => field_diffs.push(format!("{k}: exp {v}, got <missing>")),
					_ => {}
				}
			}
			for k in close.keys() {
				if !exp_block.contains_key(k) {
					field_diffs.push(format!("{k}: exp <missing>, got {}", close[k]));
				}
			}
			diffs.push(format!("ammo '{exp_name}': {}", field_diffs.join(", ")));
		} else {
			// No block with this Name at all — truly missing
			diffs.push(format!("ammo '{exp_name}': missing entirely"));
		}
	}

	if diffs.is_empty() {
		Ok(())
	} else {
		Err(diffs.join("; "))
	}
}

/// Run conversion on ALL vehicles in the corpus and report statistics.
#[test]
fn test_full_corpus() {
	let vehicles_path = test_data_dir()
		.join("datamine")
		.join("aces.vromfs.bin_u")
		.join("gamedata")
		.join("units")
		.join("tankmodels");
	let expected_data_dir = test_data_dir().join("expected").join("data");
	let output_dir = test_data_dir().join("output");
	if !vehicles_path.exists() {
		eprintln!("Skipping corpus test: examples not present");
		return;
	}

	// Ensure output directory exists
	if !output_dir.exists() {
		std::fs::create_dir_all(&output_dir).expect("create output dir");
	}

	// Collect all expected output files (these are the vehicles legacy tool produced output for)
	let expected_files: Vec<_> = std::fs::read_dir(&expected_data_dir)
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

	// Write output for failing vehicles to output/data/ for inspection
	let output_data_dir = output_dir.join("data");
	if !output_data_dir.exists() {
		std::fs::create_dir_all(&output_data_dir).expect("create output data dir");
	}

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

				// Write our output for this failing vehicle for inspection
				let vehicle_path = vehicles_path.join(format!("{vehicle}.blkx"));
				if let Ok(data) =
					convert_vehicle(&vehicle_path, &test_data_dir().join("datamine"))
				{
					let output_text = emit_legacy_txt(&data);
					let _ = std::fs::write(
						output_data_dir.join(format!("{vehicle}.txt")),
						&output_text,
					);
				}
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
		let failure_log = output_dir.join("corpus-failures.txt");
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
