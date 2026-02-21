# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.2.0] - 2026-02-20

### Added

- **Rust `fcsgen` pipeline**: Vehicle/weapon parsing, datamine extraction, and ballistic computation now run via the Rust-based `fcsgen` tool, replacing the legacy C# pipeline (#33, #34, #46, #48, #50).
- **One-click generation**: A single "Generate Sights" button runs the full extract → convert → ballistic pipeline. No more multi-step workflow (#47, #52).
- **War Thunder datamine extraction**: The app extracts data directly from your game install using the `wt_blk` crate — no external tools or pre-generated files needed (#2, #48).
- **Sensitivity and scroll wheel indicator**: A label now shows whether each sight type requires scroll wheel binding and sensitivity slider setup (#43, #66).
- **Version number in title bar**: The window title now shows the current version (#44, #63).
- **Build provenance attestation**: Release ZIPs include SLSA provenance so you can verify they were built from this repo (`gh attestation verify`) (#15).

### Changed

- **Self-contained build**: The app ships as a single compressed executable (~49 MB) with the .NET runtime bundled — no separate .NET install required (#39, #67).
- **Output folder renamed**: Default output folder changed from `UserSights` to `Output` to avoid confusion with the game's UserSights directory (#64).
- **In-memory datamine processing**: Extracted data stays in memory by default (use `--write-datamine` for debugging), eliminating ~150 MB of intermediate files (#59).
- **Ballistic engine rewrite**: Uses a density lookup table, shell-level memoization, and rayon parallelism — corpus time dropped from ~2 minutes to ~25 seconds, bit-for-bit identical to C# reference (#51).
- **Per-sight-type output**: Generated sights are written to subdirectories by sight type (`Output/{Sight}/Vehicle/`) for easier file management (#52).
- **CI/CD overhaul**: Release workflow split into build + publish jobs, safe with immutable releases. Auto-generates release notes from merged PRs (#15).
- **Documentation rewrite**: New README covering download, setup, sight types, sensitivity configuration, and caveats (#42).

### Removed

- Dropped all pre-generated `assets/` payloads (Datamine, Data, Ballistic, Localization CSV dumps, and placeholder UserSights) now that the extractor + converter regenerate them on demand (#49).

### Fixed

- Improved projectile parsing fidelity: APDS armor power series, Cx array averaging, `/name/short` fallbacks, case-sensitive laser checks, modification `commonWeapons`, ATGM belts, rocket DeMarre values, and unarmed vehicles (#19 follow-ups).
- Asset packaging paths and case-insensitive ignore list matching for the extraction pipeline.

## [2.1.3] - 2025-12-20

### Added

- Updated pregenerated data files for War Thunder 2.53.0.19 (Line of Contact)

### Changed

- Updated README to reflect new `UserSights` folder locations as of War Thunder 2.53 Line of Contact update.

## [2.1.2] - 2025-11-22

### Added

- Updated pregenerated data files for War Thunder 2.51.0.46 update (R400 added, localization changes).

## [2.1.1] - 2025-11-11

### Added

- Updated pregenerated data files for War Thunder 2.51.0.18 "Spearhead" update.

### Fixed

- Language selector now defaults to English, preventing freezing if "Make Sights" button is pressed without selecting a language.

## [2.1.0] - 2025-11-10

### Added

- Reenabled Luch, Luch Lite, and Sector sights in the UI for generation.

### Fixed

- Fixed crash when generating Luch Lite sights due to `napalm_tank` weapon type on `ussr_to_55`.
- Fixed debug build configuration not set to generate debug symbols.

## [2.0.3] - 2025-11-09

### Added

- Updated documentation to reflect current architecture, rewrite plans, and repo conventions.

### Fixed

- Fixed crash when generating Tochka sights with languages other than English, French, Italian, or German with index out of range exception.

### Changed

- Updated language selection logic to fall back to English or string ids rather than crashing if expected strings are not found.

## [2.0.2] - 2025-10-29

### Fixed

- enabling immutable releases was a mistake (oops)

## [2.0.1] - 2025-10-29

### Fixed

- Fix release build process (I hope)

## [2.0.0] - 2025-10-29

Project cleanup and modernization. Builds now distributed via GitHub Releases, upgraded to .NET 10 (preview), and many other housekeeping improvements.

### Added

- Automated GitHub Actions workflow for building and releasing
- Proper .gitignore for .NET projects
- Standardized project structure
- Build provenance attestation and SHA256 checksums for releases
- PowerShell script (`Update-Datamine.ps1`) for extracting data from local War Thunder install via [wt_ext_cli](https://github.com/Warthunder-Open-Source-Foundation/wt_ext_cli)

### Changed

- Upgraded to .NET 10 (preview)
- Cleaned up publish configuration (single-file, framework-dependent deployment)
- Updated README with clear installation and usage instructions
- Removed binaries from repository (now distributed via GitHub Releases)
- Reorganized asset structure and build output handling
- Updated project to use proper MSBuild targets for asset copying

### Removed

- Removed committed build artifacts (.vs, bin, obj directories)
- Removed unnecessary project files and cruft from original upload

### Fixed

- General stability improvements (it actually works now lol)
- Proper asset path handling for build and publish outputs
- Fixed empty directory preservation in publish output

## [1.6.231215] - 2024-02-03

Original release by [Assin127](https://live.warthunder.com/user/58909037/). Last version before project was taken over for maintenance and cleanup.

<!-- Versions -->
[2.2.0]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.2.0
[2.1.3]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.3
[2.1.2]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.2
[2.1.1]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.1
[2.1.0]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.0
[2.0.3]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.3
[2.0.2]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.2
[2.0.1]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.1
[2.0.0]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.0
[1.6.231215]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v1.6.231215
