//! Unified `run` command: extract → convert → ballistic in one invocation.
//!
//! Replaces the old three-step CLI workflow (`extract` + `convert` + `ballistic`)
//! with a single command that pipes data in-memory where possible.
//!
//! `Data/*.txt` files are still written (the C# sight generator reads them).
//! `Ballistic/{vehicle}/{shell}.txt` files are still written.

use std::collections::HashMap;
use std::path::Path;

use fcsgen_core::ballistic::{compute_ballistic, should_skip};
use fcsgen_core::parser::data::from_projectile;
use fcsgen_core::{convert_vehicle, emit_legacy_txt};

use crate::extract;

/// Configuration for the unified pipeline.
pub struct PipelineConfig<'a> {
	pub game_path: &'a Path,
	pub output: &'a Path,
	pub sensitivity: f64,
	pub ignore_file: Option<&'a Path>,
	pub filter: Option<&'a [String]>,
	pub skip_extract: bool,
	pub skip_ballistic: bool,
}

/// Run the full pipeline: extract → convert → ballistic.
#[allow(clippy::too_many_lines)]
pub fn run_pipeline(cfg: &PipelineConfig<'_>) {
	let datamine_dir = cfg.output.join("Datamine");
	let data_dir = cfg.output.join("Data");
	let ballistic_dir = cfg.output.join("Ballistic");

	// ── Step 1: Extract ────────────────────────────────────────────────
	if cfg.skip_extract {
		eprintln!("Step 1/3: Skipping extraction (--skip-extract)");
	} else {
		eprintln!("Step 1/3: Extracting datamine...");
		extract::run_extract(cfg.game_path, &datamine_dir, cfg.ignore_file, false);
	}

	// ── Step 2+3: Convert + Ballistic (in-memory pipeline) ─────────────
	let aces_root = datamine_dir.join("aces.vromfs.bin_u");
	let tankmodels = aces_root.join("gamedata").join("units").join("tankmodels");

	if !tankmodels.exists() {
		eprintln!(
			"Error: tankmodels directory not found at {}",
			tankmodels.display()
		);
		eprintln!("Run without --skip-extract to populate the datamine first.");
		std::process::exit(1);
	}

	// Create output directories
	for dir in [&data_dir, &ballistic_dir] {
		if let Err(e) = std::fs::create_dir_all(dir) {
			eprintln!("Error: cannot create directory {}: {e}", dir.display());
			std::process::exit(1);
		}
	}

	// Collect vehicle files
	let mut vehicles: Vec<_> = std::fs::read_dir(&tankmodels)
		.expect("read tankmodels")
		.filter_map(Result::ok)
		.filter(|e| e.path().extension().is_some_and(|ext| ext == "blkx"))
		.filter(|e| {
			if let Some(filter) = cfg.filter {
				let stem = e.path().file_stem().unwrap().to_string_lossy().to_string();
				filter.iter().any(|f| f == &stem)
			} else {
				true
			}
		})
		.collect();

	vehicles.sort_by_key(std::fs::DirEntry::file_name);

	let total = vehicles.len();
	let mut converted = 0;
	let mut skipped = 0;
	let mut convert_failed = 0;
	let mut shells_written = 0;
	let mut ballistic_errors = 0;

	eprintln!(
		"Step 2/3: Converting {total} vehicles (+ ballistic, sensitivity={})",
		cfg.sensitivity,
	);
	eprintln!("  Data:      {}", data_dir.display());
	if !cfg.skip_ballistic {
		eprintln!("  Ballistic: {}", ballistic_dir.display());
	}
	eprintln!();

	for entry in &vehicles {
		let path = entry.path();
		let name = path.file_stem().unwrap().to_string_lossy().to_string();

		// Convert vehicle from datamine
		let data = match convert_vehicle(&path, &datamine_dir) {
			Ok(d) => d,
			Err(e) => {
				eprintln!("CONVERT ERROR {name}: {e}");
				convert_failed += 1;
				continue;
			},
		};

		if !data.is_armed() {
			skipped += 1;
			continue;
		}

		// Write Data/{vehicle}.txt (still needed by C# sight generator)
		let txt = emit_legacy_txt(&data);
		let data_path = data_dir.join(format!("{name}.txt"));
		if let Err(e) = std::fs::write(&data_path, &txt) {
			eprintln!("WRITE ERROR {name}: {e}");
			convert_failed += 1;
			continue;
		}

		converted += 1;

		// ── In-memory ballistic computation ────────────────────────────
		if cfg.skip_ballistic {
			continue;
		}

		// Bridge Projectile → DataProjectile
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

		let vehicle_dir = ballistic_dir.join(&name);
		let mut any_written = false;

		for &idx in last_by_name.values() {
			let dp = &data_projectiles[idx];

			if let Some(content) = compute_ballistic(dp, cfg.sensitivity) {
				if content.is_empty() {
					continue;
				}

				// Ensure vehicle subdirectory exists
				if !any_written {
					if let Err(e) = std::fs::create_dir_all(&vehicle_dir) {
						eprintln!("DIR ERROR {name}: {e}");
						ballistic_errors += 1;
						break;
					}
					any_written = true;
				}

				let filename = format!("{}.txt", dp.output_name);
				let file_path = vehicle_dir.join(&filename);

				if let Err(e) = std::fs::write(&file_path, &content) {
					eprintln!("WRITE ERROR {name}/{filename}: {e}");
					ballistic_errors += 1;
				} else {
					shells_written += 1;
				}
			}
		}
	}

	eprintln!();
	eprintln!("Done: {converted} converted, {skipped} skipped (unarmed), {convert_failed} convert errors");
	if !cfg.skip_ballistic {
		eprintln!("      {shells_written} ballistic tables written, {ballistic_errors} ballistic errors");
	}
}

