//! Unified `run` command: extract → convert → ballistic in one invocation.
//!
//! Replaces the old three-step CLI workflow (`extract` + `convert` + `ballistic`)
//! with a single command that pipes data in-memory where possible.
//!
//! Vehicles are processed in parallel via [`rayon`], with a shared
//! [`BallisticCache`] (backed by `DashMap`) for cross-vehicle shell
//! deduplication.
//!
//! `Data/*.txt` files are still written (the C# sight generator reads them).
//! `Ballistic/{vehicle}/{shell}.txt` files are still written.

use std::collections::HashMap;
use std::path::Path;

use rayon::prelude::*;

use fcsgen_core::ballistic::{BallisticCache, compute_ballistic_cached, should_skip};
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
	pub jobs: usize,
	pub skip_extract: bool,
	pub skip_ballistic: bool,
}

/// Per-vehicle statistics returned from each parallel work unit.
///
/// Accumulated via `merge` in a rayon `reduce` step — no shared mutable
/// state required.
#[derive(Default)]
struct VehicleStats {
	converted: usize,
	skipped: usize,
	convert_failed: usize,
	shells_written: usize,
	ballistic_errors: usize,
	cache_hits: usize,
	cache_misses: usize,
}

impl VehicleStats {
	fn merge(mut self, other: Self) -> Self {
		self.converted += other.converted;
		self.skipped += other.skipped;
		self.convert_failed += other.convert_failed;
		self.shells_written += other.shells_written;
		self.ballistic_errors += other.ballistic_errors;
		self.cache_hits += other.cache_hits;
		self.cache_misses += other.cache_misses;
		self
	}
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

	// Cross-vehicle ballistic cache: shells with identical physics share
	// the same trajectory output regardless of which vehicle fires them.
	// DashMap provides lock-free concurrent reads with fine-grained sharded
	// locking on writes — ideal for the 80% cache-hit rate we see.
	let ballistic_cache: BallisticCache = BallisticCache::new();

	// Configure rayon thread pool
	let thread_count = if cfg.jobs > 0 {
		cfg.jobs
	} else {
		std::thread::available_parallelism()
			.map(std::num::NonZero::get)
			.unwrap_or(1)
	};

	if cfg.jobs > 0 {
		rayon::ThreadPoolBuilder::new()
			.num_threads(thread_count)
			.build_global()
			.ok(); // Ignore if already initialized (e.g. in tests)
	}

	eprintln!(
		"Step 2/3: Converting {total} vehicles (+ ballistic, sensitivity={}, jobs={thread_count})",
		cfg.sensitivity,
	);
	eprintln!("  Data:      {}", data_dir.display());
	if !cfg.skip_ballistic {
		eprintln!("  Ballistic: {}", ballistic_dir.display());
	}
	eprintln!();

	let sensitivity = cfg.sensitivity;
	let skip_ballistic = cfg.skip_ballistic;

	let stats = vehicles
		.par_iter()
		.map(|entry| {
			let mut vs = VehicleStats::default();
			let path = entry.path();
			let name = path.file_stem().unwrap().to_string_lossy().to_string();

			// Convert vehicle from datamine
			let data = match convert_vehicle(&path, &datamine_dir) {
				Ok(d) => d,
				Err(e) => {
					eprintln!("CONVERT ERROR {name}: {e}");
					vs.convert_failed += 1;
					return vs;
				},
			};

			if !data.is_armed() {
				vs.skipped += 1;
				return vs;
			}

			// Write Data/{vehicle}.txt (still needed by C# sight generator)
			let txt = emit_legacy_txt(&data);
			let data_path = data_dir.join(format!("{name}.txt"));
			if let Err(e) = std::fs::write(&data_path, &txt) {
				eprintln!("WRITE ERROR {name}: {e}");
				vs.convert_failed += 1;
				return vs;
			}

			vs.converted += 1;

			// ── In-memory ballistic computation ────────────────────────
			if skip_ballistic {
				return vs;
			}

			// Bridge Projectile → DataProjectile
			let data_projectiles: Vec<_> = data
				.projectiles
				.iter()
				.map(from_projectile)
				.collect();

			// Deduplicate by output_name: keep last occurrence (matching
			// C#'s `File.WriteAllText` overwrite semantics).
			let mut last_by_name: HashMap<String, usize> = HashMap::new();
			for (idx, dp) in data_projectiles.iter().enumerate() {
				if !should_skip(&dp.normalized_type) {
					last_by_name.insert(dp.output_name.clone(), idx);
				}
			}

			let vehicle_dir = ballistic_dir.join(&name);
			let mut dir_created = false;

			for &idx in last_by_name.values() {
				let dp = &data_projectiles[idx];

				let (result, hit) =
					compute_ballistic_cached(dp, sensitivity, &ballistic_cache);
				if hit {
					vs.cache_hits += 1;
				} else {
					vs.cache_misses += 1;
				}

				if let Some(content) = result {
					if content.is_empty() {
						continue;
					}

					// Ensure vehicle subdirectory exists
					if !dir_created {
						if let Err(e) = std::fs::create_dir_all(&vehicle_dir) {
							eprintln!("DIR ERROR {name}: {e}");
							vs.ballistic_errors += 1;
							break;
						}
						dir_created = true;
					}

					let filename = format!("{}.txt", dp.output_name);
					let file_path = vehicle_dir.join(&filename);

					if let Err(e) = std::fs::write(&file_path, &content) {
						eprintln!("WRITE ERROR {name}/{filename}: {e}");
						vs.ballistic_errors += 1;
					} else {
						vs.shells_written += 1;
					}
				}
			}

			vs
		})
		.reduce(VehicleStats::default, VehicleStats::merge);

	eprintln!();
	eprintln!(
		"Done: {} converted, {} skipped (unarmed), {} convert errors",
		stats.converted, stats.skipped, stats.convert_failed,
	);
	if !cfg.skip_ballistic {
		let total_lookups = stats.cache_hits + stats.cache_misses;
		eprintln!(
			"      {} ballistic tables written, {} ballistic errors",
			stats.shells_written, stats.ballistic_errors,
		);
		eprintln!(
			"      Cache: {} unique / {total_lookups} total ({} hits, {:.0}% reuse)",
			stats.cache_misses,
			stats.cache_hits,
			if total_lookups > 0 { 100.0 * stats.cache_hits as f64 / total_lookups as f64 } else { 0.0 },
		);
	}
}

