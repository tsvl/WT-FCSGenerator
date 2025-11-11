# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
[2.0.3]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.3
[2.0.2]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.2
[2.0.1]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.1
[2.0.0]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v2.0.0
[1.6.231215]: https://github.com/tsvl/WT-FCSGenerator/releases/tag/v1.6.231215
