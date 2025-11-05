# WT-FCSGenerator — High-level Overview

This project generates user sights for War Thunder ground vehicles by transforming datamined game assets into ready-to-use sight scripts. It follows a simple three-stage pipeline driven by three buttons in the WinForms UI:

1) Convert Datamine → produces intermediate per-vehicle Data files
2) Make Ballistic → produces per-shell ballistic tables
3) Make Sights → renders sight .blk scripts from the ballistics and localization

The pipeline is intentionally file-based, which makes the stages loosely coupled and easy to re-run independently.

## Repository layout

- src/ — WinForms app and generation logic
  - Form1.cs — UI + orchestration for all three stages and helpers (ballistics, HE penetration, pairing rules)
  - TochkaAM.cs — Tochka-SM2 sight generator
  - Luch.cs, Luch_Lite.cs, Duga.cs, Duga2.cs, Sector.cs — other sight generators
- docs/ — documentation
  - formats.md — file formats for Data/, Ballistic/, and UserSights/
  - datamine-to-data.md — extraction rules from datamine to Data/
  - cli-stage1.md — CLI spec for Stage 1 rewrite
  - refactor-plan.md — plan for rewriting the pipeline in Rust

- assets/ — utility scripts and pregenerated data files.

## The three-stage pipeline

### 1) Convert Datamine (Button1_Click)

Input:

- War Thunder datamine exports, primarily .blkx files under aces.vromfs.bin_u/gamedata/units/tankmodels and weapon/rocket modules.

Process:

- Parse vehicle .blkx to find weapon and rocket module paths, cockpit FOV (zoom), and laser presence.
- Parse weapon/rocket .blkx to enumerate projectiles and extract physical parameters: mass, caliber, muzzle velocity, drag (Cx), explosive mass/type, DeMarre coefficients, and (for APDS-FS) armor power series.
- Normalize values (e.g., average Cx arrays if a list is present).
- Resolve human-readable names from Localization CSVs (e.g., units_weaponry.csv).

Output:

- `Data/{vehicle}.txt` — compact text file with a header (paths, zoom, HasLaser) and multiple Name blocks with projectile parameters. See docs/formats.md for schema.

### 2) Make Ballistic (Button3_Click)

Input:

- `Data/{vehicle}.txt` from stage 1.

Process:

- For each Name block (skipping rockets, smoke, practice, etc.), compute a trajectory and penetration curve by numerical integration using the code’s Ballistic(...) helper. AP/APHE/APDS use DeMarre; APDS-FS may use supplied ArmorPower arrays. HE shells have zero penetration but still produce a trajectory.

Output:

- `Ballistic/{vehicle}/{shell}.txt` — a tabular file with three columns: distance (m), time (s), penetration (mm). See docs/formats.md for details.

### 3) Make Sights (Button2_Click)

Input:

- Ballistic tables from stage 2, `Data/{vehicle}.txt` metadata, `Localization/FCS.csv`, and `Localization/units_weaponry.csv` (only needed for Tochka and possibly Luch sights).

Process:

- For the selected sight family (Tochka-SM2, Luch, Luch Lite, Duga, Duga-2, Sector), read ballistic tables and options from the UI, compute geometry (ticks, labels, preemptive lines), and render a sight script string via the sight’s Create(...) method.
- Variants (e.g., DoubleShells, Laser, Rocket, Howitzer) adjust data sources or drawing logic.

Output:

- `UserSights/{vehicle}/{sight}.blk` — ready-to-use sight files.

## Core components

- Form1.cs
  - Button1_Click — Convert Datamine
  - Button3_Click — Make Ballistic
  - Button2_Click — Make Sights
  - Helpers:
    - Ballistic(...) — trajectory + DeMarre penetration integration (handles AP, APHE, APDS, APDS-FS)
    - HePenetration(mass, type) — explosive equivalence to HE penetration
    - CanUseDoubleShell(...) — rules for pairing two shells in a “double” sight

- Sight generators (Create(...))
  - Tochka-SM2 (TochkaAM.cs): rich feature set including Double/Laser/Rocket/Howitzer variants
  - Luch, Luch Lite, Duga, Duga-2, Sector: analogous Create(...) methods for different layouts

## Data flow between folders

- Datamine → Data — per-vehicle consolidated parameters (txt)
- Data → Ballistic — per-shell ballistic tables (txt)
- Ballistic + Localization (+ options) → UserSights — sight .blk scripts

Each stage can be re-run independently if its inputs change.
