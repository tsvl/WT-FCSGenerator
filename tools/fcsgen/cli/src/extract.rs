//! Datamine extraction from War Thunder VROMFS archives.
//!
//! Uses the `wt_blk` crate to open `aces.vromfs.bin` and `lang.vromfs.bin`,
//! unpack the files we need (tank models, weapons, localization CSVs), and
//! either return them in memory or write them to disk.
//!
//! The default mode (`run_extract_in_memory`) keeps aces files in memory
//! and only writes lang CSVs to disk, avoiding the 150 MB intermediate dump.
//! Use `--write-datamine` to also persist the full aces extraction.

use std::collections::{HashMap, HashSet};
use std::io::BufRead;
use std::path::Path;

use fcsgen_core::Datamine;
use wt_blk::vromf::{BlkOutputFormat, File as VromfFile, VromfUnpacker};

/// Marker filename written to the extraction output directory after a
/// successful pipeline run.  Contains the WT version string and sensitivity
/// so we can skip re-processing when nothing has changed.
pub const VERSION_MARKER: &str = ".fcsgen-version";

/// Result of an in-memory extraction.
pub struct ExtractionResult {
	/// In-memory aces files: normalized path → JSON string.
	/// Keys are lowercase with forward slashes, relative to aces.vromfs.bin_u/,
	/// e.g. `"gamedata/units/tankmodels/us_m1_abrams.blkx"`.
	pub datamine: Datamine,

	/// Sorted list of vehicle stems (without .blkx extension).
	pub vehicle_names: Vec<String>,

	/// War Thunder version string extracted from the archive metadata.
	pub version: String,
}

/// Extract datamine into memory, only writing lang CSVs to disk.
///
/// If `write_datamine` is true, also writes all aces .blkx files to disk
/// (matching the old behaviour for debugging/testing).
///
/// Returns an [`ExtractionResult`] with all aces files in memory.
pub fn run_extract_in_memory(
	game_path: &Path,
	output: &Path,
	ignore_file: Option<&Path>,
	write_datamine: bool,
) -> ExtractionResult {
	// --- Validate archive paths ---
	let aces_bin = game_path.join("aces.vromfs.bin");
	let lang_bin = game_path.join("lang.vromfs.bin");

	if !aces_bin.exists() {
		eprintln!("Error: aces.vromfs.bin not found at {aces_bin:?}");
		eprintln!("Make sure the path points to the War Thunder installation directory.");
		std::process::exit(1);
	}
	if !lang_bin.exists() {
		eprintln!("Error: lang.vromfs.bin not found at {lang_bin:?}");
		eprintln!("Make sure the path points to the War Thunder installation directory.");
		std::process::exit(1);
	}

	// --- Open aces archive ---
	let aces_file = VromfFile::new(&aces_bin).unwrap_or_else(|e| {
		eprintln!("Error: failed to read {aces_bin:?}: {e}");
		std::process::exit(1);
	});
	let aces_unpacker = VromfUnpacker::from_file(&aces_file, false).unwrap_or_else(|e| {
		eprintln!("Error: failed to parse {aces_bin:?}: {e}");
		std::process::exit(1);
	});

	// --- Version check ---
	let version = aces_unpacker.latest_version().unwrap_or_else(|e| {
		eprintln!("Warning: could not read version from archive: {e}");
		None
	});

	let version_str = version.map_or_else(|| "unknown".to_owned(), |v| v.to_string());

	eprintln!("Extracting datamine (version {version_str})...");

	// --- Load ignore list ---
	let ignore_set = ignore_file.map_or_else(HashSet::new, load_ignore_list);

	// --- Unpack aces archive ---
	let aces_files = aces_unpacker
		.unpack_all(Some(BlkOutputFormat::Json), false)
		.unwrap_or_else(|e| {
			eprintln!("Error: failed to unpack {aces_bin:?}: {e}");
			std::process::exit(1);
		});

	// --- Filter and collect aces files ---
	let aces_root = output.join("aces.vromfs.bin_u");

	let tankmodels_prefix = Path::new("gamedata/units/tankmodels");
	let weapons_prefix = Path::new("gamedata/weapons/groundmodels_weapons");

	let mut datamine: Datamine = HashMap::new();
	let mut vehicle_names: Vec<String> = Vec::new();
	let mut written_tankmodels: HashSet<String> = HashSet::new();
	let mut tankmodel_count: u32 = 0;
	let mut weapon_count: u32 = 0;

	for file in &aces_files {
		let file_path = file.path();

		// tankmodels: top-level .blk files only (no subdirectories)
		if let Ok(rel) = file_path.strip_prefix(tankmodels_prefix) {
			// Top-level only: the relative path should be just a filename
			if rel.parent().is_some_and(|p| p != Path::new("")) {
				continue;
			}
			let filename = rel.to_string_lossy();
			if !filename.ends_with(".blk") {
				continue;
			}

			// Check ignore list (compare stem, case-insensitive)
			let stem = filename.strip_suffix(".blk").unwrap_or(&filename);
			if ignore_set.contains(&stem.to_lowercase()) {
				continue;
			}

			// Renamed key: .blk → .blkx
			let blkx_filename = format!("{filename}x");
			let key = format!(
				"{}/{}",
				tankmodels_prefix.to_string_lossy(),
				blkx_filename
			);

			// Store in memory
			let content = String::from_utf8_lossy(file.buf()).into_owned();
			datamine.insert(key, content);
			vehicle_names.push(stem.to_string());

			// Optionally write to disk
			if write_datamine {
				let dest = aces_root.join(tankmodels_prefix).join(&blkx_filename);
				write_file(&dest, file.buf());
				written_tankmodels.insert(blkx_filename);
			}

			tankmodel_count += 1;
			continue;
		}

		// weapons: all files under groundmodels_weapons
		if file_path.starts_with(weapons_prefix) {
			// Normalized key: lowercase path with .blkx extension
			let key_path = if file_path.extension().is_some_and(|ext| ext == "blk") {
				file_path.with_extension("blkx")
			} else {
				file_path.to_path_buf()
			};
			let key = key_path
				.to_string_lossy()
				.replace('\\', "/")
				.to_lowercase();

			// Store in memory
			let content = String::from_utf8_lossy(file.buf()).into_owned();
			datamine.insert(key, content);

			// Optionally write to disk
			if write_datamine {
				let dest = aces_root.join(&key_path);
				write_file(&dest, file.buf());
			}

			weapon_count += 1;
		}
	}

	// Delete stale tankmodel files on disk when writing
	if write_datamine {
		let tankmodels_dir = aces_root.join(tankmodels_prefix);
		if tankmodels_dir.is_dir()
			&& let Ok(entries) = std::fs::read_dir(&tankmodels_dir)
		{
			for entry in entries.filter_map(Result::ok) {
				let name = entry.file_name().to_string_lossy().into_owned();
				if name.ends_with(".blkx") && !written_tankmodels.contains(&name) {
					let _ = std::fs::remove_file(entry.path());
				}
			}
		}
	}

	// Sort vehicle names for deterministic processing order
	vehicle_names.sort();

	// --- Extract lang archive ---
	extract_lang(game_path, output);

	eprintln!(
		"Extracted {tankmodel_count} tankmodels, {weapon_count} weapons (version {version_str})"
	);

	ExtractionResult {
		datamine,
		vehicle_names,
		version: version_str,
	}
}

/// Run the full extraction pipeline, writing all files to disk.
///
/// This is the legacy behaviour used by the standalone `extract` subcommand
/// and `--write-datamine` mode.
pub fn run_extract(
	game_path: &Path,
	output: &Path,
	ignore_file: Option<&Path>,
	force: bool,
) {
	// --- Validate archive paths ---
	let aces_bin = game_path.join("aces.vromfs.bin");

	if !aces_bin.exists() {
		eprintln!("Error: aces.vromfs.bin not found at {aces_bin:?}");
		eprintln!("Make sure the path points to the War Thunder installation directory.");
		std::process::exit(1);
	}

	// --- Version check (skip if up-to-date) ---
	let aces_file = VromfFile::new(&aces_bin).unwrap_or_else(|e| {
		eprintln!("Error: failed to read {aces_bin:?}: {e}");
		std::process::exit(1);
	});
	let aces_unpacker = VromfUnpacker::from_file(&aces_file, false).unwrap_or_else(|e| {
		eprintln!("Error: failed to parse {aces_bin:?}: {e}");
		std::process::exit(1);
	});

	let version = aces_unpacker.latest_version().unwrap_or_else(|e| {
		eprintln!("Warning: could not read version from archive: {e}");
		None
	});

	let version_str = version.map_or_else(|| "unknown".to_owned(), |v| v.to_string());

	let marker_path = output.join(VERSION_MARKER);
	if !force {
		if let Ok(cached) = std::fs::read_to_string(&marker_path) {
			if cached.trim() == version_str {
				eprintln!("Already up-to-date (version {version_str})");
				return;
			}
		}
	}

	// Full extraction with disk writes
	run_extract_in_memory(game_path, output, ignore_file, true);
}

/// Extract lang CSVs from lang.vromfs.bin.
fn extract_lang(game_path: &Path, output: &Path) {
	let lang_bin = game_path.join("lang.vromfs.bin");

	let lang_file = VromfFile::new(&lang_bin).unwrap_or_else(|e| {
		eprintln!("Error: failed to read {lang_bin:?}: {e}");
		std::process::exit(1);
	});
	let lang_unpacker = VromfUnpacker::from_file(&lang_file, false).unwrap_or_else(|e| {
		eprintln!("Error: failed to parse {lang_bin:?}: {e}");
		std::process::exit(1);
	});

	// CSVs are plain text, no BLK decoding needed
	let lang_files = lang_unpacker
		.unpack_all(None, false)
		.unwrap_or_else(|e| {
			eprintln!("Error: failed to unpack {lang_bin:?}: {e}");
			std::process::exit(1);
		});

	let mut lang_count: u32 = 0;
	let lang_targets: [&str; 2] = ["lang/units.csv", "lang/units_weaponry.csv"];
	let lang_root = output.join("lang.vromfs.bin_u");

	for file in &lang_files {
		let file_path = file.path();
		let path_str = file_path.to_string_lossy();

		// Normalize path separators for comparison
		let normalized: String = path_str.replace('\\', "/");

		for target in &lang_targets {
			if normalized == *target {
				let dest = lang_root.join(target);
				write_file(&dest, file.buf());
				lang_count += 1;
			}
		}
	}

	eprintln!("Extracted {lang_count} lang files");
}

/// Write `data` to `path`, creating parent directories as needed.
fn write_file(path: &Path, data: &[u8]) {
    if let Some(parent) = path.parent() {
        if let Err(e) = std::fs::create_dir_all(parent) {
            eprintln!("Error: cannot create directory {parent:?}: {e}");
            std::process::exit(1);
        }
    }
    if let Err(e) = std::fs::write(path, data) {
        eprintln!("Error: cannot write {path:?}: {e}");
        std::process::exit(1);
    }
}

/// Load a vehicle ignore list from a file.
///
/// Each line is a vehicle ID (optionally quoted, with `#` comment lines).
/// Returns a set of vehicle IDs (lowercased, quotes and `.blkx` extension stripped).
fn load_ignore_list(path: &Path) -> HashSet<String> {
    let file = match std::fs::File::open(path) {
        Ok(f) => f,
        Err(e) => {
            eprintln!("Warning: cannot read ignore file {path:?}: {e}");
            return HashSet::new();
        },
    };

    let reader = std::io::BufReader::new(file);
    reader
        .lines()
        .filter_map(Result::ok)
        .map(|line| line.trim().to_owned())
        .filter(|line| !line.is_empty() && !line.starts_with('#'))
        .map(|line| {
            // Strip surrounding quotes (legacy format compat)
            if line.starts_with('"') && line.ends_with('"') && line.len() >= 2 {
                line[1..line.len() - 1].to_owned()
            } else {
                line
            }
        })
        // Strip .blkx extension if present (legacy format compat)
        .map(|s| s.strip_suffix(".blkx").unwrap_or(&s).to_owned())
        // Lowercase for case-insensitive matching (archive names are lowercase)
        .map(|s| s.to_lowercase())
        .collect()
}
