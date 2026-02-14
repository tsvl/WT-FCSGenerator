//! CLI orchestrator for the `ballistic` subcommand.
//!
//! Walks `Data/*.txt` files, parses each, runs the trajectory simulation
//! for every projectile, and writes `Ballistic/{vehicle}/{shell}.txt`.

use std::path::Path;

use fcsgen_core::ballistic::{compute_ballistic, should_skip};
use fcsgen_core::parser::data::parse_data_file;

/// Run the ballistic computation pipeline.
///
/// # Arguments
/// * `input`       – Directory containing `Data/*.txt` files (Stage 1 output).
/// * `output`      – Directory to write `Ballistic/{vehicle}/{shell}.txt` into.
/// * `sensitivity` – Mouse sensitivity value (0 < s ≤ 1, typically 0.50).
/// * `filter`      – Optional list of vehicle IDs to process.
pub fn run_ballistic(
	input: &Path,
	output: &Path,
	sensitivity: f64,
	filter: Option<&[String]>,
) {
	if !input.exists() {
		eprintln!("Error: input directory not found at {input:?}");
		std::process::exit(1);
	}

	if let Err(e) = std::fs::create_dir_all(output) {
		eprintln!("Error: cannot create output directory: {e}");
		std::process::exit(1);
	}

	// Collect *.txt files from input directory
	let mut files: Vec<_> = std::fs::read_dir(input)
		.expect("read input directory")
		.filter_map(|e| e.ok())
		.filter(|e| e.path().extension().is_some_and(|ext| ext == "txt"))
		.filter(|e| {
			if let Some(filter) = filter {
				let stem = e.path().file_stem().unwrap().to_string_lossy().to_string();
				filter.iter().any(|f| f == &stem)
			} else {
				true
			}
		})
		.collect();

	files.sort_by_key(std::fs::DirEntry::file_name);

	let total = files.len();
	let mut processed = 0;
	let mut shells_written = 0;
	let mut failed = 0;

	eprintln!("Computing ballistic tables for {total} vehicles (sensitivity={sensitivity})");
	eprintln!("Input:  {input:?}");
	eprintln!("Output: {output:?}");
	eprintln!();

	for entry in &files {
		let path = entry.path();
		let vehicle_id = path
			.file_stem()
			.unwrap()
			.to_str()
			.unwrap_or("unknown");

		let data = match parse_data_file(&path) {
			Ok(d) => d,
			Err(e) => {
				eprintln!("PARSE ERROR {vehicle_id}: {e}");
				failed += 1;
				continue;
			},
		};

		let vehicle_dir = output.join(vehicle_id);
		let mut any_written = false;

		for proj in &data.projectiles {
			if should_skip(&proj.normalized_type) {
				continue;
			}

			if let Some(content) = compute_ballistic(proj, sensitivity) {
				if content.is_empty() {
					continue;
				}

				// Ensure vehicle subdirectory exists
				if !any_written {
					if let Err(e) = std::fs::create_dir_all(&vehicle_dir) {
						eprintln!("DIR ERROR {vehicle_id}: {e}");
						failed += 1;
						break;
					}
					any_written = true;
				}

				let filename = format!("{}.txt", proj.output_name);
				let file_path = vehicle_dir.join(&filename);

				if let Err(e) = std::fs::write(&file_path, &content) {
					eprintln!("WRITE ERROR {vehicle_id}/{filename}: {e}");
					failed += 1;
				} else {
					shells_written += 1;
				}
			}
		}

		if any_written {
			processed += 1;
		}
	}

	eprintln!();
	eprintln!(
		"Done: {processed} vehicles, {shells_written} shell tables written, {failed} errors"
	);
}
