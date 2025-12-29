# fcsgen

Rust CLI tool for War Thunder FCS (Fire Control System) generation.

This tool is being developed to replace the legacy .NET WinForms application,
starting with datamine conversion (Stage 1) and expanding to ballistics and
sight generation.

## Status

🚧 **Under development** — this workspace is a skeleton for now.

## Building

```sh
cd tools/fcsgen
cargo build --release
```

The binary will be at `target/release/fcsgen.exe` (Windows) or `target/release/fcsgen` (Linux/macOS).

## Documentation

- [CLI Stage 1 Specification](../../docs/cli-stage1.md) — `convert-datamine` command
- [Refactor Plan](../../docs/refactor-plan.md) — overall roadmap
- [Datamine → Data](../../docs/datamine-to-data.md) — extraction rules
- [File Formats](../../docs/formats.md) — output format specifications

## Crate Structure

- **`fcsgen-core`** — Core library with parsing, ballistics, and rendering logic
- **`fcsgen`** (cli) — Command-line interface that orchestrates the core library
