# WT-FCSGenerator — High-level Overview

This project generates user sights for War Thunder ground vehicles by transforming game assets into ready-to-use sight scripts. It uses a three-stage pipeline:

1. **Extract & Convert** — extract datamine from game files, produce per-vehicle `Data/*.txt`
2. **Ballistic** — compute per-shell trajectory and penetration tables
3. **Sight Generation** — render `.blk` sight scripts from ballistic data, localization, and user options

Stages 1 and 2 are implemented in Rust (`tools/fcsgen/`) and run as an external CLI tool. Stage 3 is implemented in C# as part of the WinForms UI (`src/`). A single "Generate Sights" button in the UI orchestrates all three stages.

## Repository layout

```
src/                        .NET 10 WinForms app (stage 3 + UI)
  Form1.cs                  UI, pipeline orchestration, stage 3 dispatch
  TochkaAM.cs               Tochka-SM2 sight generator
  Luch.cs, Luch_Lite.cs     Luchs sight generators
  Duga.cs, Duga2.cs         Duga sight generators
  Sector.cs                 Sector sight generator

tools/fcsgen/               Rust workspace (stages 1 + 2)
  cli/src/
    main.rs                 CLI entry point (clap subcommands)
    extract.rs              Stage 1: VROMFS extraction + datamine parsing
    run.rs                  Unified pipeline: extract → convert → ballistic
  core/src/
    stage1.rs               Stage 1: datamine → Data/*.txt conversion
    stage2.rs               Stage 2: Data/*.txt → Ballistic/*/*.txt

assets/                     Localization CSVs, ignore list, utility scripts
  Data/                     Extracted per-vehicle parameter files (generated)
  Ballistic/                Per-shell ballistic tables (generated)
  Localization/             FCS.csv + units_weaponry.csv
  Datamine/                 Extracted datamine cache (generated, gitignored)

docs/                       Documentation
  overview.md               This file
  formats.md                Intermediate file format specs
  datamine-to-data.md       Extraction rules reference (stage 1)
  sights.md                 Sight family summaries
  stage3-form1-core.md      Reverse-engineering notes for stage 3 rewrite
```

## Pipeline architecture

### Stage 1 — Extract & Convert (Rust)

The `fcsgen` CLI extracts game data directly from VROMFS archives in the War Thunder install directory using the `wt_blk` crate. No manual datamine export is required.

**Process:**

1. Open `aces.vromfs.bin` from the game path (header-only for version check)
2. Check freshness: compare game version + sensitivity against a cached `.fcsgen-version` marker — if up-to-date, skip extraction entirely
3. Extract and parse vehicle `.blkx` files to find weapon/rocket module paths, optics FOV, and laser presence
4. Parse weapon/rocket `.blkx` to extract projectile parameters (mass, caliber, velocity, drag, explosive, DeMarre coefficients, armor power series)
5. Resolve human-readable names from `units.csv` localization
6. Emit `Data/{vehicle}.txt` files

Vehicles listed in `assets/ignore.txt` are skipped.

**Output:** `Data/{vehicle}.txt` — see [formats.md](formats.md) for schema. For detailed extraction rules, see [datamine-to-data.md](datamine-to-data.md).

### Stage 2 — Ballistic (Rust)

For each vehicle's projectiles, computes trajectory and penetration curves by numerical integration.

**Process:**

1. Parse `Data/{vehicle}.txt` projectile blocks (skipping rockets, smoke, practice, etc.)
2. Integrate trajectory using drag model (Cx coefficient, atmospheric constants)
3. Compute penetration at each distance step: DeMarre formula for AP/APHE/APCR, distance-indexed arrays for APDS/APFSDS, HE penetration formula for explosive shells
4. Apply sensitivity-based distance step quantization for sight tick spacing

**Output:** `Ballistic/{vehicle}/{shell}.txt` — tabular distance/time/penetration. See [formats.md](formats.md).

### Stages 1+2 — Unified Pipeline

In normal operation, stages 1 and 2 run together via `fcsgen run`:

```
fcsgen run --game-path <wt_dir> --output <app_dir> --sensitivity <value>
```

The unified pipeline runs **in-memory**: extracted datamine data is piped directly from stage 1 to stage 2 without writing intermediate `.blkx` files to disk. Vehicle processing is parallelized with rayon.

A version marker (`.fcsgen-version`) caches the game version and sensitivity value. On subsequent runs, if both match, the pipeline is skipped entirely — making repeated sight generation instant.

### Stage 3 — Sight Generation (C#)

After the Rust pipeline completes, the WinForms app generates sight `.blk` files using the legacy C# sight generators.

**Process:**

1. Iterate `Data/*.txt` files, filtered by user-selected nations
2. For each vehicle, load ballistic tables from `Ballistic/{vehicle}/`
3. Apply shell eligibility rules (skip smoke, practice, etc.) and pairing logic (`CanUseDoubleShell`)
4. Dispatch to the selected sight family's `Create(...)` method
5. Write `.blk` files to `UserSights/{sight_type}/{vehicle}/`

**Sight families:** Tochka-SM2, Luch, Luch Lite, Duga, Duga-2, Sector. See [sights.md](sights.md) for details.

**Output:** `UserSights/{sight_type}/{vehicle}/{sight}.blk` — ready-to-use War Thunder sight files.

## User-facing flow

1. Select sight type, language, nation(s), and options in the UI
2. Set War Thunder game path and sensitivity
3. Click **Generate Sights**
4. The app validates inputs, runs `fcsgen run` (stages 1+2), then generates sights (stage 3)
5. Copy the generated `UserSights/` contents to the War Thunder sight directory

## CLI subcommands

The `fcsgen` CLI (`tools/fcsgen/`) provides four subcommands:

| Command | Purpose | Typical use |
| --------- | --------- | ------------- |
| `run` | Full pipeline: extract → convert → ballistic | Called by the WinForms UI |
| `extract` | Standalone VROMFS extraction | Debugging / manual extraction |
| `convert` | Standalone datamine → Data/*.txt | Debugging / reprocessing |
| `ballistic` | Standalone Data/*.txt → Ballistic/ | Debugging / reprocessing |

`run` is the primary entry point. The others exist for debugging and incremental use.

## Key dependencies

**Rust** (`tools/fcsgen/`):

- `wt_blk` — VROMFS archive reading and BLK parsing
- `rayon` — parallel vehicle processing
- `dashmap` — concurrent memoization cache for ballistic calculations
- `clap` — CLI argument parsing
- `serde` / `serde_json` — JSON deserialization of extracted BLK data

**C#** (`src/`):

- .NET 10.0 WinForms — UI and sight generation
