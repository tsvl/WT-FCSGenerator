# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Introduced the Rust-based `fcsgen` Stage 1 pipeline (`tools/fcsgen`) with vehicle and weapon parsers, legacy emitters, and integration tests plus a `convert` CLI subcommand for datamine-to-`Data/*.txt` conversion (#34).
- Added a Stage 0 War Thunder datamine extraction step driven by the `wt_blk` crate, exposed via the `fcsgen extract` CLI subcommand and wired into the WinForms UI so users only need to point at their game install (#2).
- Checked-in `tools/fcsgen/test_data/` corpus fixtures (datamine inputs + expected legacy outputs) so remote CI and contributors can run the Stage 1 integration suite without local War Thunder files.
- Added the Stage 2 ballistic engine rewrite with corpus-level tests plus the new `fcsgen run` CLI that chains extract → convert → ballistic in one go (replacing the legacy C# computation).

### Fixed

- Improved projectile parsing fidelity: APDS armor power series extraction, Cx array averaging, `/name/short` fallbacks, case-sensitive laser checks, and handling of modification `commonWeapons`, ATGM belts, rocket DeMarre values, and unarmed vehicles (stage 1 follow-ups for issue #19).
- Corrected asset packaging paths and made the datamine ignore list matching case-insensitive so the extraction pipeline actually honors `assets/ignore.txt`.

### Changed

- Added Rust formatting configuration so the new workspace adheres to repository style.
- WinForms UI now shells out to `fcsgen run`, replacing the legacy inline datamine parser so Stage 1 lives entirely in Rust (closes the multi-button workflow gap).
- Datamine processing now stays in-memory by default (with `--write-datamine` for debugging), eliminating ~150 MB of intermediates and simplifying the WinForms output layout.
- Ballistic pipeline now uses a density lookup table, shell-level memoization cache, and rayon-based parallelism, cutting corpus time from ~2 minutes to ~25 seconds while staying bit-for-bit identical to the C# reference.

### Removed

- Dropped all pre-generated `assets/` payloads (Datamine, Data, Ballistic, Localization CSV dumps, and placeholder UserSights) now that the extractor + converter regenerate them on demand.

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
[2.1.3]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.3
[2.1.2]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.2
[2.1.1]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.1
[2.1.0]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.1.0
[2.0.3]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.3
[2.0.2]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.2
[2.0.1]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.1
[2.0.0]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.0
[1.6.231215]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v1.6.231215
