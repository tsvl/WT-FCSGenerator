//! Unified `run` command: extract → convert → ballistic in one invocation.
//!
//! Replaces the old three-step CLI workflow (`extract` + `convert` + `ballistic`)
//! with a single command that pipes data in-memory where possible.
//!
//! By default, datamine files (.blkx) are kept in memory and never written to
//! disk — only `Data/*.txt`, `Ballistic/{vehicle}/{shell}.txt`, and lang CSVs
//! are persisted.  Use `--write-datamine` to also dump the full extraction.
//!
//! Vehicles are processed in parallel via [`rayon`], with a shared
//! [`BallisticCache`] (backed by `DashMap`) for cross-vehicle shell
//! deduplication.

use std::collections::HashMap;
use std::path::Path;

use rayon::prelude::*;
use wt_blk::vromf::{File as VromfFile, VromfUnpacker};

use fcsgen_core::ballistic::{BallisticCache, compute_ballistic_cached, should_skip};
use fcsgen_core::parser::data::from_projectile;
use fcsgen_core::{convert_vehicle, convert_vehicle_in_memory, emit_legacy_txt};

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
	pub write_datamine: bool,
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

/// Check whether the pipeline output is already up-to-date.
///
/// Reads the version marker from `datamine_dir` (two-line format:
/// `version\nsensitivity`) and compares against the current archive version
/// and requested sensitivity.  Also verifies that `data_dir` contains at
/// least one `.txt` file and `ballistic_dir` exists.
///
/// Returns the cached version string if up-to-date, `None` otherwise.
fn check_up_to_date(
	game_path: &Path,
	datamine_dir: &Path,
	data_dir: &Path,
	ballistic_dir: &Path,
	sensitivity: f64,
	skip_ballistic: bool,
) -> Option<String> {
	// Read marker file (two-line format: "version\nsensitivity")
	let marker_path = datamine_dir.join(extract::VERSION_MARKER);
	let marker_content = std::fs::read_to_string(&marker_path).ok()?;
	let mut lines = marker_content.lines();
	let cached_version = lines.next()?.trim();
	let cached_sensitivity: f64 = lines.next()?.trim().parse().ok()?;

	// Compare sensitivity
	if (cached_sensitivity - sensitivity).abs() > f64::EPSILON {
		return None;
	}

	// Verify Data/ has at least one .txt file
	let has_data_files = std::fs::read_dir(data_dir)
		.ok()?
		.filter_map(Result::ok)
		.any(|e| {
			e.path()
				.extension()
				.is_some_and(|ext| ext == "txt")
		});
	if !has_data_files {
		return None;
	}

	// Verify Ballistic/ exists (unless ballistic is skipped)
	if !skip_ballistic && !ballistic_dir.is_dir() {
		return None;
	}

	// Read archive version without unpacking
	let aces_bin = game_path.join("aces.vromfs.bin");
	let aces_file = VromfFile::new(&aces_bin).ok()?;
	let aces_unpacker = VromfUnpacker::from_file(&aces_file, true).ok()?;
	let version = aces_unpacker.latest_version().ok()??;
	let version_str = version.to_string();

	if cached_version == version_str {
		Some(version_str)
	} else {
		None
	}
}

/// Write the version+sensitivity marker after a successful pipeline run.
fn write_marker(datamine_dir: &Path, version: &str, sensitivity: f64) {
	if let Err(e) = std::fs::create_dir_all(datamine_dir) {
		eprintln!("Warning: cannot create Datamine dir for marker: {e}");
		return;
	}
	let marker_path = datamine_dir.join(extract::VERSION_MARKER);
	let content = format!(
		"{version}\n{sensitivity}",
	);
	if let Err(e) = std::fs::write(&marker_path, content) {
		eprintln!("Warning: failed to write version marker: {e}");
	}
}

/// Run the full pipeline: extract → convert → ballistic.
#[allow(clippy::too_many_lines)]
pub fn run_pipeline(cfg: &PipelineConfig<'_>) {
	let datamine_dir = cfg.output.join("Datamine");
	let data_dir = cfg.output.join("Data");
	let ballistic_dir = cfg.output.join("Ballistic");

	// Create output directories
	for dir in [&data_dir, &ballistic_dir] {
		if let Err(e) = std::fs::create_dir_all(dir) {
			eprintln!("Error: cannot create directory {}: {e}", dir.display());
			std::process::exit(1);
		}
	}

	// ── Freshness check: skip if version+sensitivity unchanged ─────────
	if !cfg.skip_extract {
		if let Some(ver) = check_up_to_date(
			cfg.game_path,
			&datamine_dir,
			&data_dir,
			&ballistic_dir,
			cfg.sensitivity,
			cfg.skip_ballistic,
		) {
			eprintln!(
				"Already up-to-date (version {ver}, sensitivity {})",
				cfg.sensitivity,
			);
			return;
		}
	}

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

	// Cross-vehicle ballistic cache
	let ballistic_cache: BallisticCache = BallisticCache::new();

	let sensitivity = cfg.sensitivity;
	let skip_ballistic = cfg.skip_ballistic;

	// ── Branch: in-memory vs disk-based extraction ─────────────────────
	if cfg.skip_extract {
		// Disk-based path: read .blkx files from a previous extraction
		eprintln!("Step 1/3: Skipping extraction (--skip-extract)");
		run_pipeline_from_disk(
			cfg,
			&datamine_dir,
			&data_dir,
			&ballistic_dir,
			&ballistic_cache,
			sensitivity,
			skip_ballistic,
			thread_count,
		);
	} else {
		// In-memory path: extract → convert → ballistic without writing .blkx
		eprintln!("Step 1/3: Extracting datamine...");
		let extraction = extract::run_extract_in_memory(
			cfg.game_path,
			&datamine_dir,
			cfg.ignore_file,
			cfg.write_datamine,
		);
		run_pipeline_in_memory(
			cfg,
			&extraction,
			&data_dir,
			&ballistic_dir,
			&ballistic_cache,
			sensitivity,
			skip_ballistic,
			thread_count,
		);

		// Write version+sensitivity marker on success
		write_marker(&datamine_dir, &extraction.version, sensitivity);
	}
}

/// Pipeline branch: process vehicles from in-memory datamine.
fn run_pipeline_in_memory(
	cfg: &PipelineConfig<'_>,
	extraction: &extract::ExtractionResult,
	data_dir: &Path,
	ballistic_dir: &Path,
	ballistic_cache: &BallisticCache,
	sensitivity: f64,
	skip_ballistic: bool,
	thread_count: usize,
) {
	// Apply vehicle filter
	let vehicle_names: Vec<&String> = extraction
		.vehicle_names
		.iter()
		.filter(|name| {
			if let Some(filter) = cfg.filter {
				filter.iter().any(|f| f == *name)
			} else {
				true
			}
		})
		.collect();

	let total = vehicle_names.len();
	let tankmodels_prefix = "gamedata/units/tankmodels";

	eprintln!(
		"Step 2/3: Converting {total} vehicles (+ ballistic, sensitivity={}, jobs={thread_count})",
		cfg.sensitivity,
	);
	eprintln!("  Data:      {}", data_dir.display());
	if !skip_ballistic {
		eprintln!("  Ballistic: {}", ballistic_dir.display());
	}
	eprintln!();

	let stats = vehicle_names
		.par_iter()
		.map(|name| {
			let mut vs = VehicleStats::default();

			// Look up vehicle content from in-memory datamine
			let key = format!("{tankmodels_prefix}/{name}.blkx");
			let vehicle_content = match extraction.datamine.get(&key) {
				Some(content) => content,
				None => {
					eprintln!("CONVERT ERROR {name}: not found in datamine");
					vs.convert_failed += 1;
					return vs;
				},
			};

			// Convert vehicle from in-memory data
			let data = match convert_vehicle_in_memory(name, vehicle_content, &extraction.datamine)
			{
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

			// Write Data/{vehicle}.txt (needed by C# sight generator)
			let txt = emit_legacy_txt(&data);
			let data_path = data_dir.join(format!("{name}.txt"));
			if let Err(e) = std::fs::write(&data_path, &txt) {
				eprintln!("WRITE ERROR {name}: {e}");
				vs.convert_failed += 1;
				return vs;
			}

			vs.converted += 1;

			// Ballistic computation
			if skip_ballistic {
				return vs;
			}

			process_ballistic(&data, name, ballistic_dir, sensitivity, ballistic_cache, &mut vs);
			vs
		})
		.reduce(VehicleStats::default, VehicleStats::merge);

	print_stats(&stats, skip_ballistic);
}

/// Pipeline branch: process vehicles from disk-based datamine.
fn run_pipeline_from_disk(
	cfg: &PipelineConfig<'_>,
	datamine_dir: &Path,
	data_dir: &Path,
	ballistic_dir: &Path,
	ballistic_cache: &BallisticCache,
	sensitivity: f64,
	skip_ballistic: bool,
	thread_count: usize,
) {
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

	eprintln!(
		"Step 2/3: Converting {total} vehicles (+ ballistic, sensitivity={}, jobs={thread_count})",
		cfg.sensitivity,
	);
	eprintln!("  Data:      {}", data_dir.display());
	if !skip_ballistic {
		eprintln!("  Ballistic: {}", ballistic_dir.display());
	}
	eprintln!();

	let stats = vehicles
		.par_iter()
		.map(|entry| {
			let mut vs = VehicleStats::default();
			let path = entry.path();
			let name = path.file_stem().unwrap().to_string_lossy().to_string();

			// Convert vehicle from disk
			let data = match convert_vehicle(&path, datamine_dir) {
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

			// Write Data/{vehicle}.txt
			let txt = emit_legacy_txt(&data);
			let data_path = data_dir.join(format!("{name}.txt"));
			if let Err(e) = std::fs::write(&data_path, &txt) {
				eprintln!("WRITE ERROR {name}: {e}");
				vs.convert_failed += 1;
				return vs;
			}

			vs.converted += 1;

			if skip_ballistic {
				return vs;
			}

			process_ballistic(&data, &name, ballistic_dir, sensitivity, ballistic_cache, &mut vs);
			vs
		})
		.reduce(VehicleStats::default, VehicleStats::merge);

	print_stats(&stats, skip_ballistic);
}

/// Compute and write ballistic tables for a single vehicle's projectiles.
fn process_ballistic(
	data: &fcsgen_core::VehicleData,
	name: &str,
	ballistic_dir: &Path,
	sensitivity: f64,
	ballistic_cache: &BallisticCache,
	vs: &mut VehicleStats,
) {
	let data_projectiles: Vec<_> = data.projectiles.iter().map(from_projectile).collect();

	// Deduplicate by output_name
	let mut last_by_name: HashMap<String, usize> = HashMap::new();
	for (idx, dp) in data_projectiles.iter().enumerate() {
		if !should_skip(&dp.normalized_type) {
			last_by_name.insert(dp.output_name.clone(), idx);
		}
	}

	let vehicle_dir = ballistic_dir.join(name);
	let mut dir_created = false;

	for &idx in last_by_name.values() {
		let dp = &data_projectiles[idx];

		let (result, hit) = compute_ballistic_cached(dp, sensitivity, ballistic_cache);
		if hit {
			vs.cache_hits += 1;
		} else {
			vs.cache_misses += 1;
		}

		if let Some(content) = result {
			if content.is_empty() {
				continue;
			}

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
}

/// Print final pipeline statistics.
fn print_stats(stats: &VehicleStats, skip_ballistic: bool) {
	eprintln!();
	eprintln!(
		"Done: {} converted, {} skipped (unarmed), {} convert errors",
		stats.converted, stats.skipped, stats.convert_failed,
	);
	if !skip_ballistic {
		let total_lookups = stats.cache_hits + stats.cache_misses;
		eprintln!(
			"      {} ballistic tables written, {} ballistic errors",
			stats.shells_written, stats.ballistic_errors,
		);
		eprintln!(
			"      Cache: {} unique / {total_lookups} total ({} hits, {:.0}% reuse)",
			stats.cache_misses,
			stats.cache_hits,
			if total_lookups > 0 {
				100.0 * stats.cache_hits as f64 / total_lookups as f64
			} else {
				0.0
			},
		);
	}
}

