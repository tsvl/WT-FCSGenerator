//! Datamine extraction from War Thunder VROMFS archives.
//!
//! Uses the `wt_blk` crate to open `aces.vromfs.bin` and `lang.vromfs.bin`,
//! unpack the files we need (tank models, weapons, localization CSVs), and
//! write them to disk in the directory structure that `fcsgen convert` expects.

use std::collections::HashSet;
use std::io::BufRead;
use std::path::Path;

use wt_blk::vromf::{BlkOutputFormat, File as VromfFile, VromfUnpacker};

/// Marker filename written to the extraction output directory after a
/// successful extraction.  Contains the WT version string so we can skip
/// re-extraction when the archive hasn't changed.
const VERSION_MARKER: &str = ".fcsgen-version";

/// Run the full extraction pipeline.
///
/// Returns `Ok(())` on success or prints diagnostics and calls
/// `process::exit(1)` on fatal errors (matching the `convert` subcommand
/// convention).
pub fn run_extract(
    game_path: &Path,
    output: &Path,
    ignore_file: Option<&Path>,
    force: bool,
) {
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

    let marker_path = output.join(VERSION_MARKER);
    if !force {
        if let Ok(cached) = std::fs::read_to_string(&marker_path) {
            if cached.trim() == version_str {
                eprintln!("Already up-to-date (version {version_str})");
                return;
            }
        }
    }

    eprintln!("Extracting datamine (version {version_str})...");

    // --- Load ignore list ---
    let ignore_set = ignore_file.map_or_else(HashSet::new, |path| load_ignore_list(path));

    // --- Unpack aces archive ---
    let aces_files = aces_unpacker
        .unpack_all(Some(BlkOutputFormat::Json), false)
        .unwrap_or_else(|e| {
            eprintln!("Error: failed to unpack {aces_bin:?}: {e}");
            std::process::exit(1);
        });

    // --- Filter and write aces files ---
    let aces_root = output.join("aces.vromfs.bin_u");

    let tankmodels_prefix = Path::new("gamedata/units/tankmodels");
    let weapons_prefix = Path::new("gamedata/weapons/groundmodels_weapons");

    let mut tankmodel_count: u32 = 0;
    let mut weapon_count: u32 = 0;
    let mut written_tankmodels: HashSet<String> = HashSet::new();

    for file in &aces_files {
        let file_path = file.path();

        // tankmodels: top-level .blk files only (no subdirectories)
        if let Ok(rel) = file_path.strip_prefix(tankmodels_prefix) {
            // Top-level only: the relative path should be just a filename (no parent components)
            if rel.parent().is_some_and(|p| p != Path::new("")) {
                continue;
            }
            let filename = rel.to_string_lossy();
            if !filename.ends_with(".blk") {
                continue;
            }

            // Rename .blk → .blkx (matching the convention expected by `fcsgen convert`)
            let blkx_filename = format!("{filename}x");

            // Check ignore list (ignore.txt uses .blkx names, case-insensitive)
            let lower = blkx_filename.to_lowercase();
            if ignore_set.contains(&lower)
            {
                continue;
            }

            // Write with .blkx extension
            let dest = aces_root.join(tankmodels_prefix).join(&blkx_filename);
            write_file(&dest, file.buf());
            written_tankmodels.insert(blkx_filename);
            tankmodel_count += 1;
            continue;
        }

        // weapons: all files under groundmodels_weapons
        if file_path.starts_with(weapons_prefix) {
            // Rename .blk → .blkx for weapons too
            let dest_path = if file_path.extension().is_some_and(|ext| ext == "blk") {
                aces_root.join(file_path.with_extension("blkx"))
            } else {
                aces_root.join(file_path)
            };
            write_file(&dest_path, file.buf());
            weapon_count += 1;
        }
    }

    // --- Delete stale tankmodel files ---
    let tankmodels_dir = aces_root.join(tankmodels_prefix);
    if tankmodels_dir.is_dir() {
        if let Ok(entries) = std::fs::read_dir(&tankmodels_dir) {
            for entry in entries.filter_map(Result::ok) {
                let name = entry.file_name().to_string_lossy().into_owned();
                if name.ends_with(".blkx") && !written_tankmodels.contains(&name) {
                    let _ = std::fs::remove_file(entry.path());
                }
            }
        }
    }

    // --- Extract lang archive ---
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
                // Write mirroring archive structure: output/lang.vromfs.bin_u/lang/<file>
                let dest = lang_root.join(target);
                write_file(&dest, file.buf());
                lang_count += 1;
            }
        }
    }

    // --- Write version marker ---
    if let Err(e) = std::fs::create_dir_all(output) {
        eprintln!("Error: cannot create output directory {output:?}: {e}");
        std::process::exit(1);
    }
    if let Err(e) = std::fs::write(&marker_path, &version_str) {
        eprintln!("Warning: failed to write version marker: {e}");
    }

    eprintln!(
        "Extracted {tankmodel_count} tankmodels, {weapon_count} weapons, {lang_count} lang files (version {version_str})"
    );
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
/// Each line is a filename (optionally quoted, with `#` comment lines).
/// Returns a set of filenames (with quotes stripped).
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
            // Strip surrounding quotes
            if line.starts_with('"') && line.ends_with('"') && line.len() >= 2 {
                line[1..line.len() - 1].to_owned()
            } else {
                line
            }
        })
        // Lowercase for case-insensitive matching (archive names are lowercase)
        .map(|s| s.to_lowercase())
        .collect()
}
