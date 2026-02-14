//! fcsgen CLI — War Thunder FCS generation tool.
//!
//! Primary command is `fcsgen run`, which performs extraction, conversion,
//! and ballistic computation in a single invocation with an in-memory
//! pipeline (no text roundtrip between conversion and ballistic stages).
//!
//! Legacy subcommands (`convert`, `extract`, `ballistic`) are retained
//! for debugging and development workflows.

mod ballistic;
mod extract;
mod run;

use std::path::PathBuf;

use clap::{Parser, Subcommand};
use fcsgen_core::{VERSION, convert_vehicle, emit_legacy_txt};

#[derive(Parser)]
#[command(name = "fcsgen", version = VERSION, about = "War Thunder FCS generation tool")]
struct Cli {
	#[command(subcommand)]
	command: Commands,
}

#[derive(Subcommand)]
enum Commands {
	/// Run the full pipeline: extract → convert → ballistic
	Run {
		/// Path to the War Thunder installation directory
		#[arg(long)]
		game_path: PathBuf,

		/// Base output directory (creates Datamine/, Data/, Ballistic/ subdirs)
		#[arg(short, long)]
		output: PathBuf,

		/// Mouse sensitivity (0 < s ≤ 1)
		#[arg(short, long, default_value_t = 0.50)]
		sensitivity: f64,

		/// Path to ignore.txt vehicle blacklist file
		#[arg(long)]
		ignore_file: Option<PathBuf>,

		/// Only process specific vehicle(s) by name (without .blkx extension)
		#[arg(long)]
		vehicle: Option<Vec<String>>,

		/// Number of parallel jobs (0 = auto-detect based on CPU count)
		#[arg(short, long, default_value_t = 0)]
		jobs: usize,

		/// Skip extraction (use existing datamine in Datamine/)
		#[arg(long, default_value_t = false)]
		skip_extract: bool,

		/// Skip ballistic computation (only extract + convert)
		#[arg(long, default_value_t = false)]
		skip_ballistic: bool,
	},

	/// Convert datamine to Data/*.txt format (legacy, prefer `run`)
	Convert {
		/// Input directory containing extracted datamine (aces.vromfs.bin_u)
		#[arg(short, long)]
		input: PathBuf,

		/// Output directory for converted .txt files
		#[arg(short, long)]
		output: PathBuf,

		/// Only convert specific vehicle(s) by name (without .blkx extension)
		#[arg(long)]
		vehicle: Option<Vec<String>>,
	},

	/// Extract datamine from War Thunder VROMFS archives (standalone)
	Extract {
		/// Path to the War Thunder installation directory
		#[arg(long)]
		game_path: PathBuf,

		/// Output directory for extracted datamine files
		#[arg(short, long)]
		output: PathBuf,

		/// Path to ignore.txt vehicle blacklist file
		#[arg(long)]
		ignore_file: Option<PathBuf>,

		/// Force re-extraction even if version matches cached marker
		#[arg(long, default_value_t = false)]
		force: bool,
	},

	/// Compute ballistic tables from Data/*.txt (legacy, prefer `run`)
	Ballistic {
		/// Input directory containing Data/*.txt files (Stage 1 output)
		#[arg(short, long)]
		input: PathBuf,

		/// Output directory for Ballistic/{vehicle}/{shell}.txt files
		#[arg(short, long)]
		output: PathBuf,

		/// Mouse sensitivity (0 < s ≤ 1)
		#[arg(short, long, default_value_t = 0.50)]
		sensitivity: f64,

		/// Only process specific vehicle(s) by name (without .txt extension)
		#[arg(long)]
		vehicle: Option<Vec<String>>,
	},
}

fn main() {
	let cli = Cli::parse();

	match cli.command {
		Commands::Run {
			game_path,
			output,
			sensitivity,
			ignore_file,
			vehicle,
			jobs,
			skip_extract,
			skip_ballistic,
		} => {
			run::run_pipeline(&run::PipelineConfig {
				game_path: &game_path,
				output: &output,
				sensitivity,
				ignore_file: ignore_file.as_deref(),
				filter: vehicle.as_deref(),
				jobs,
				skip_extract,
				skip_ballistic,
			});
		},
		Commands::Convert {
			input,
			output,
			vehicle,
		} => {
			run_convert(&input, &output, vehicle.as_deref());
		},
		Commands::Extract {
			game_path,
			output,
			ignore_file,
			force,
		} => {
			extract::run_extract(
				&game_path,
				&output,
				ignore_file.as_deref(),
				force,
			);
		},
		Commands::Ballistic {
			input,
			output,
			sensitivity,
			vehicle,
		} => {
			ballistic::run_ballistic(
				&input,
				&output,
				sensitivity,
				vehicle.as_deref(),
			);
		},
	}
}

fn run_convert(input: &PathBuf, output: &PathBuf, filter: Option<&[String]>) {
	// Input should be the aces.vromfs.bin_u directory itself
	let tankmodels = input.join("gamedata").join("units").join("tankmodels");

	if !tankmodels.exists() {
		eprintln!("Error: tankmodels directory not found at {tankmodels:?}");
		eprintln!("Expected structure: <input>/gamedata/units/tankmodels/");
		eprintln!("(input should be the aces.vromfs.bin_u directory)");
		std::process::exit(1);
	}

	// convert_vehicle expects the parent of aces.vromfs.bin_u
	let datamine_root = input.parent().unwrap_or(input);

	// Create output directory
	if let Err(e) = std::fs::create_dir_all(output) {
		eprintln!("Error: cannot create output directory: {e}");
		std::process::exit(1);
	}

	// Collect vehicle files
	let vehicles: Vec<_> = std::fs::read_dir(&tankmodels)
		.expect("read tankmodels")
		.filter_map(|e| e.ok())
		.filter(|e| e.path().extension().is_some_and(|ext| ext == "blkx"))
		.filter(|e| {
			if let Some(filter) = filter {
				let stem = e.path().file_stem().unwrap().to_string_lossy().to_string();
				filter.iter().any(|f| f == &stem)
			} else {
				true
			}
		})
		.collect();

	let total = vehicles.len();
	let mut converted = 0;
	let mut skipped = 0;
	let mut failed = 0;

	eprintln!("Converting {total} vehicles from {tankmodels:?}");
	eprintln!("Output: {output:?}");
	eprintln!();

	for entry in &vehicles {
		let path = entry.path();
		let name = path.file_stem().unwrap().to_string_lossy();

		match convert_vehicle(&path, datamine_root) {
			Ok(data) if data.is_armed() => {
				let txt = emit_legacy_txt(&data);
				let out_path = output.join(format!("{name}.txt"));

				if let Err(e) = std::fs::write(&out_path, &txt) {
					eprintln!("WRITE ERROR {name}: {e}");
					failed += 1;
				} else {
					converted += 1;
				}
			},
			Ok(_) => {
				// Unarmed vehicle (no projectiles found), skip output
				skipped += 1;
			},
			Err(e) => {
				eprintln!("CONVERT ERROR {name}: {e}");
				failed += 1;
			},
		}
	}

	eprintln!();
	eprintln!("Done: {converted} converted, {skipped} skipped (unarmed), {failed} failed");
}
