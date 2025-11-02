# CLI: Stage 1 — convert-datamine

A standalone tool to replace the WinForms Button1 step. Reads War Thunder datamine and emits:

- Legacy `Data/<vehicle>.txt` files (for current pipeline)
- Optional JSON intermediate with full fidelity (for future stages)

## Command

`fcsgen convert-datamine --datamine-root <dir> [--vehicle <id> | --vehicles <glob> ...] [options]`

## Options

- --datamine-root <dir> (required)
  Path containing `aces.vromfs.bin_u/gamedata/...`.

- --vehicle <id>
  Vehicle id (basename of `units/tankmodels/<id>.blkx`). May be repeated.

- --vehicles <glob>
  Glob under `units/tankmodels/*.blkx` (e.g., `ussr_*`, `*_abrams_*`). May be repeated.

- --lang-csv <file>
  Path to `units.csv` to resolve `<LangName2>`; if omitted, emitter uses the vehicle basename.

- --out-data <dir> (default: `Data/`)
  Destination for legacy `.txt` outputs.

- --out-json <dir>
  Destination for JSON intermediates (if `--emit` includes json).

- --emit `legacy|json|both` (default: `both`)

- --threads <n> (default: number of logical CPUs)

- --log-level `info|debug|trace` (default: `info`)

## Behavior

- Applies robust extraction rules from `docs/datamine-to-data.md` (JSONPaths, weapon selection heuristics, APDS/APFSDS series).
- Emits deterministic output (stable ordering) for diffability.
- Writes legacy `.txt` conforming to `docs/formats.md`.

## Exit codes

- 0 — success
- 2 — input/IO error (missing files, unreadable paths)
- 3 — parse/schema error (unexpected JSON structure)
- 4 — emit error (failed to write outputs)
- 5 — partial success (some vehicles failed; see logs)

## Logging

- Human-friendly by default.
- Optional structured JSON lines when `--log-level trace` for CI ingestion.

## Examples

Process a single vehicle and emit legacy `.txt` only:

`fcsgen convert-datamine --datamine-root D:\\WT-Datamine --vehicle ussr_bmp_2m --emit legacy --out-data .\\Data`

Process all Chinese vehicles, emit both JSON and legacy outputs in parallel:

`fcsgen convert-datamine --datamine-root D:\\WT-Datamine --vehicles cn_* --out-data .\\Data --out-json .\\.fcs\\json --threads 8`

## Validation workflow

- Compare outputs to `examples/Data/*.txt` using a small diff harness.
- Start with a representative corpus (1–2 per nation) covering: primary gun + APHE, APDS/APFSDS with series, ATGM carriers (2 rockets), vehicle with secondary optics, and an SPAA.
