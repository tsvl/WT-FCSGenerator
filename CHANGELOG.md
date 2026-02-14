# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.2.0] - 2026-02-13

### Added

- **Rust datamine extraction and ballistic engine** — Stages 1 (datamine → Data/*.txt) and 2 (ballistic calculation) are now implemented in Rust (`tools/fcsgen/`), replacing ~1100 lines of C# with a faster, more robust implementation.
- **Automatic game data extraction** — The app now extracts data directly from War Thunder's VROMFS archives. No more manual datamine exports or pre-generated data files.
- **In-memory pipeline** — Extracted datamine data is piped directly from stage 1 to stage 2 without writing intermediate files to disk.
- **Version + sensitivity freshness check** — A `.fcsgen-version` marker caches the game version and sensitivity value. Subsequent runs skip the pipeline entirely if nothing changed.
- **Parallel processing** — Vehicle processing is parallelized with rayon, reducing pipeline runtime from ~79s to ~14s on a typical desktop.
- **Ballistic memoization cache** — Cross-vehicle DashMap cache avoids recomputing identical shells shared between vehicles (~45% hit rate).
- **Golden-diff test suites** — Stage 1 (1168 vehicles), stage 2 (3807 shells), and combined pipeline integration tests.
- **Sight-type subdirectories** — Generated sights are now organized into `UserSights/{sight_type}/{vehicle}/` instead of a flat structure.
- **Stage 3 reverse-engineering documentation** — Detailed notes on how the legacy sight generation code works, for future rewrite reference.

### Changed

- **Single "Generate Sights" button** — The three-button workflow (Convert Datamine, Make Ballistic, Make Sights) is unified into a single button with input validation.
- **Input validation** — The app now validates that a sight type is selected, the game path contains `aces.vromfs.bin`, and `fcsgen.exe` is present before proceeding.
- Pre-generated Data/ and Ballistic/ assets removed from the repository (now generated on-demand from game files).
- Documentation overhauled: updated overview, removed obsolete planning docs, updated extraction rules reference to point to Rust implementation.

### Removed

- Button1 (Convert Datamine) and Button3 (Make Ballistic) — replaced by the integrated pipeline.
- ModOptic feature — secondary optics (`_ModOptic.txt`) are no longer generated.
- Pre-generated asset data files from the repository.
- Obsolete documentation: `cli-stage1.md`, `known-issues.md`, `refactor-plan.md`.

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
