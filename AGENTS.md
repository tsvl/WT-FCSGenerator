# AGENTS.md

## Project overview

WT-FCSGenerator produces user sights for War Thunder ground vehicles. A three-stage pipeline extracts game data, computes ballistic tables, and renders `.blk` sight scripts. See [docs/overview.md](docs/overview.md) for detailed architecture and data flow.

## Repository layout

```
src/                   C# / .NET 10 WinForms app — UI + sight generation (stage 3)
tools/fcsgen/          Rust workspace — datamine extraction + ballistic computation (stages 1–2)
  cli/                 CLI binary (clap subcommands, pipeline orchestration)
  core/                Library crate (parsing, conversion, ballistic math)
assets/                Runtime data (localization CSVs, ignore list, generated intermediates)
docs/                  Architecture docs, format specs, sight family notes
```

**Important:** `src/` is exclusively C#/.NET. `tools/fcsgen/` is exclusively Rust. Don't look for Rust code in `src/` or C# code in `tools/`.

## Working practices

- Create a branch per change and open a PR to `main` for review.
- After making changes, update:
  1. `CHANGELOG.md` — add entries under `## [Unreleased]`.
  2. `AGENTS.md` — if the change affects anything documented here (e.g., directory structure).
  3. `CONTRIBUTING.md` — if the change affects development workflow or setup.
  4. Relevant docs in `docs/` if the change affects architecture or formats.

## Versioning and releases

- The app version lives in the `VERSION` file (semver, e.g., `2.2.1`).
- To release: bump `VERSION`, set the `[Unreleased]` changelog heading to the new version and date, commit to `main`, then push a `v{version}` tag. The CI release workflow (`.github/workflows/release.yml`) handles the rest — it validates the tag matches `VERSION`, builds, packages, attests provenance, and publishes a GitHub Release.
