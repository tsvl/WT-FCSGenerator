# Stage 1 — Datamine → Data/{vehicle}.txt (authoritative mapping)

> **Implementation status:** This document was originally written as a reference for the C# implementation in `Form1.Button1_Click`. Stage 1 has since been **rewritten in Rust** — the current implementation lives in `tools/fcsgen/core/src/stage1.rs` (conversion logic) and `tools/fcsgen/cli/src/extract.rs` (VROMFS extraction). The extraction rules documented here remain the authoritative reference for *what* the code does and *why*, even though the *how* has changed significantly.

This document exhaustively describes how the Stage 1 "Convert Datamine" step parses War Thunder datamine (.blkx files, deserialized as JSON) and writes the intermediate `Data/{vehicle}.txt` file. It covers:

- Every field written to the Data file
- Exactly where each field comes from in the datamine JSON and the expected type
- All fallbacks, defaults, and edge cases
- Known hardcoded constants used later that should be sourced from datamine

## Output shape at a glance

The Data file starts with a header, then an arbitrary number of projectile blocks. Example (abridged):

```data
WeaponPath:gameData/Weapons/groundModels_weapons/30mm_2A42_user_cannon.blkx
RocketPath:gameData/Weapons/groundModels_weapons/152mm_9M133_launcher_user_cannon.blkx
ZoomIn:6.14
ZoomOut:29.8
HasLaser

Name:30mm_UOF8
Type:he_frag_i
BulletMass:0.389
BallisticCaliber:0.03
Speed:960.0
Cx:0.29841
ExplosiveMass:0.049
ExplosiveType:a_ix_2
…
```

## Header fields (per-vehicle)

- WeaponPath
  - Source: vehicle file `aces.vromfs.bin_u/gamedata/units/tankmodels/{vehicle}.blkx`
  - Extracted when a line contains `"groundModels_weapons"`.
  - Code takes the 4th quoted token (`line.Split('"')[3]`) and appends `x` to force `.blkx` extension.
  - Notes: relies on string scanning; path must appear quoted on one line.

- RocketPath (0–2 occurrences)
  - Source: same vehicle file.
  - When encountering `"triggerGroup": "special"`, the very next line is expected to contain the rocket module path. The code reads that next line’s 4th quoted token and appends `x`.
  - At most two unique RocketPath entries are recorded; duplicate values are discarded.
  - Notes: assumes the path is on the immediately following line after the triggerGroup line.

- ZoomIn, ZoomOut (primary optics)
  - Source: within the `"cockpit"` block of the vehicle file.
  - If the value is an array, the code scans forward until a line with digits and uses the first numeric element.
  - If it’s a scalar, it takes the token right of `:` and strips spaces/commas.
  - Notes: values are preserved as plain numbers in degrees (no unit label).

- ZoomIn/Out second pair (secondary optics)
  - Source: the next `"cockpit"` block encountered.
  - Behavior mirrors the primary pair; if present, the app also generates an additional `{vehicle}_ModOptic.txt` where the primary ZoomIn/Out are replaced by this second pair.

- HasLaser
  - Source: any line in the vehicle file containing the substring `laser`.
  - Type: presence-only flag (no value).
  - Notes: very broad heuristic; may catch unrelated keys that contain the substring.

## Projectile blocks (per module/bullet/rocket)

Stage 1 aggregates projectiles from up to three module files:

- The primary WeaponPath module
- RocketPath (optional)
- RocketPath2 (optional)

The code scans each module’s JSON line-by-line. For each `"bullet"` or `"rocket"` entry, it attempts to determine whether the module is actually used by the current vehicle before parsing details. This “presence” check is string-based and brittle (details below).

Each block starts with:

- Name
  - Primary source: `"bulletName"`. If the value is an array (weapon path case), it takes the first string element. Rocket path code assumes scalar (array case is NOT handled there).
  - Fallback: `bulletType + "/name/short"` if `bulletName` is missing.
  - Pre-filter: When building the presence check, the module key above the `"bullet"` line is also compared to the vehicle data with regional prefixes stripped: `_cn_/_fr_/_germ_/_il_/_it_/_jp_/_sw_/_uk_/_us_/_ussr_` → `_`.

- Type
  - Source: `"bulletType"` raw string, written as-is (e.g., `he_frag_i`, `ap_t`, `apds_autocannon`, `apds_fs_long_tank`, `atgm_tandem_tank`, `atgm_vt_fuze_tank`, …).
  - Notes: later stages may normalize `apds_fs*` to `apds_fs` when reading.

Then, the following fields may appear depending on availability in the module JSON:

- BulletMass
  - Source: `"mass"` (numeric)

- BallisticCaliber
  - Source: prefers `"ballisticCaliber"` if present; otherwise `"caliber"` (only if the line contains `"caliber": 0.`). Values copied as numeric strings.

- Speed
  - Source: `"speed"`; may be overwritten by a later `"endSpeed"` if present (last one wins).

- Cx
  - Source: `"Cx"`
  - Scalar case: take token after `:`.
  - Array case: average all numeric tokens between `[` and `]` (across lines); result rounded to 4 decimals.
  - Fallback default: `0.38` if not found or average is empty.

- ExplosiveMass
  - Source: `"explosiveMass"`

- ExplosiveType
  - Source: `"explosiveType"`

- DamageMass
  - Source: `"damageMass"`

- DamageCaliber
  - Source: `"damageCaliber"`

- demarrePenetrationK, demarreSpeedPow, demarreMassPow, demarreCaliberPow
  - Source: the corresponding `"demarre…"` keys (only parsed in the WeaponPath module loop).

- ArmorPower
  - Source: `"armorPower"` (single scalar). Seen in many rockets/ATGMs.

- APDS armor power series (distance-indexed)
  - Source: `"ArmorPower{distance}m"` keys (e.g., ArmorPower0m, 100m, 500m, …, 10000m)
  - Extraction rule: ONLY when `bulletType.Split('_')[0] == "apds"` (i.e., types beginning with `apds`), the following fields are added if present:
    - APDS0, APDS100, APDS500, APDS1000, APDS1500, APDS2000, APDS2500, APDS3000, APDS3500, APDS4000, APDS4500, APDS10000
  - Notes: This does NOT trigger for `apds_fs*` types. If the datamine provides series for APFSDS, they will be missed by the current code and won’t be available to Stage 2 — a known gap to address in a rewrite.

At the end of each block assembly, commas are removed from the accumulated text (`BulletInfo = BulletInfo.Replace(",", "")`).

## Module presence check (brittle)

Before parsing a `"bullet"`/`"rocket"` block, the code attempts to ensure the module is used by the vehicle:

- WeaponPath pass: walks upward from the `"bullet"` line to find the nearest quoted `name: value` line; sets `BulletName` to its key; then checks if `TankData.Contains(BulletName)`; applies regional prefix stripping and checks again.
- RocketPath pass: similar but simpler check looking at the immediately preceding line; no array handling for `bulletName`.

If the name is not found in the vehicle data, the block is skipped.

This approach is error-prone because it depends on textual proximity and coarse substring matching. Prefer reading the actual assignment structure (e.g., ammo arrays) in a future implementation.

## Vehicle name and file naming

After gathering header + blocks, Stage 1 loads `lang.vromfs.bin_u\\lang\\units.csv` to resolve the vehicle’s localization key:

- It searches for a line containing `{vehicle_basename}_shop` (case-insensitive).
- Takes the first column (strips quotes and `_shop`) as `LangName2`.
- Writes the Data file as `Data/{LangName2}.txt`.
- If a second Zoom pair exists, writes `Data/{LangName2}_ModOptic.txt` with ZoomIn/Out replaced by the secondary values.

Note: This makes Stage 1 depend on a localization CSV and (subtly) on its structure.

## Known hardcoded constants and tables (not from datamine)

While not written into the Data file, Stage 2/3 rely on constants embedded in code:

- TNT equivalence factors for ExplosiveType → equivalent mass (HePenetration)
  - Large static mapping in `HePenetration(ExplosiveMass, ExplosiveType)`. Example keys: `a_ix_2`, `octol`, `torpex`, `pbxn_3`, etc. Likely outdated relative to datamine.
- HE penetration mass→mm table (HePenetration)
  - Piecewise linear mapping from mass (kg) to penetration (mm).
- APHE penetration reduction vs filler ratio (PenByExpl)
- Sub-caliber mass mixing factor (PenBySubcaliber) for APCR/APDS
- Default DeMarre parameters if missing (K=0.9, speedPow=1.43, massPow=0.71, caliberPow=1.07)
- Default Cx fallback (0.38) when absent
- Environmental constants for trajectory integration (g, p, T) and scroll step formula

Recommendation: Surface these as data from the datamine where available, or at least version them in a config, not code.

## Edge cases and brittleness summary

- bulletName arrays handled for WeaponPath, but not in RocketPath/RocketPath2 parsing.
- Speed can be taken from either `speed` or `endSpeed` (last wins), but intent is unclear.
- Caliber parsing depends on a string match `"caliber": 0.`; if formatting changes, it may be missed.
- Cx arrays are averaged correctly, but numeric token parsing is line-based and may include unintended numbers if formatting changes.
- APDS armor series are only captured for types starting with `apds`, not `apds_fs*`.
- `HasLaser` is set by any `"laser"` substring, not a specific capability flag.
- Module presence relies on substring matching of names rather than structured relationships.

## JSON intermediate (historical note)

> **Status:** The pipeline now runs fully in-memory — extracted datamine data is piped directly from stage 1 to stage 2 via Rust structs without any intermediate files. The JSON intermediate described below was considered during planning but was superseded by the in-memory approach. This section is preserved for reference.

The original proposal was to introduce a JSON intermediate while maintaining compatibility:

- vehicle
  - id: `{vehicle_basename}`
  - key: `{LangName2}`
  - zooms: `[{ in, out, label: "primary" }, { in, out, label: "secondary" }]` (optional second)
  - hasLaser: boolean
  - modules: { weaponPath, rocketPaths: [] }
- projectiles: array of
  - name: `bulletName`
  - type: `bulletType` (raw)
  - mass: number
  - ballisticCaliber: number
  - speed: { muzzle: number, end?: number }
  - cx: { value: number, source: "scalar"|"array" }
  - explosive?: { mass: number, type: string }
  - damage?: { mass: number, caliber: number }
  - demarre?: { K, speedPow, massPow, caliberPow }
  - armorPower?: number
  - armorPowerSeries?: `[{ distance: m, value: mm }]`
  - source: { modulePath, presentInVehicle: true/false, presenceMethod: "structured"|"heuristic" }

Mapping rules:

- Prefer `ballisticCaliber`; fallback to `caliber`.
- For arrays (cx, bulletName): take first logical entry; preserve the array too if helpful.
- For APDS/APFSDS: collect all `ArmorPower{N}m` into `armorPowerSeries` regardless of the type prefix.

From this JSON, emit the current .txt format deterministically (for interop) until Stage 2/3 are updated. See also CLI details in `docs/cli-stage1.md`.

## What to include now to reduce later lookups

- Record the resolved `LangName2` (vehicle key) explicitly.
- Add the `bulletType + "/name/short"` and/or a stable localization key per projectile so Stage 3 doesn’t need to re-scan `units_weaponry.csv`.
- Include `armorPowerSeries` whenever present (both APDS and APFSDS families).
- Capture both zoom pairs in a structured way instead of duplicating files.

With these tweaks, Stage 2/3 could operate purely on the intermediate without re-reading game CSVs, and you can swap in a robust JSON reader instead of ad-hoc string scans.

## Robust extraction strategy (implemented in Rust)

> **Status:** These strategies are now implemented in the Rust codebase (`tools/fcsgen/core/src/stage1.rs` and `tools/fcsgen/cli/src/extract.rs`) using `serde_json` for structured parsing. The JSONPaths below serve as a reference for understanding the extraction logic.

### Vehicle header fields

- ZoomIn / ZoomOut (primary optics)
  - Path: `$.cockpit.zoomInFov`, `$.cockpit.zoomOutFov`
  - Shapes: `number` or `array<number>`
  - Rule: if array, take the first numeric element; if scalar, take value. Units are degrees.

- ZoomIn / ZoomOut (secondary optics)
  - Path: if `$.cockpit` is an array of objects, take index 1 (`$.cockpit[1].zoomInFov`, `$.cockpit[1].zoomOutFov`).
  - If nested under a named alternate sight (e.g., `$.commanderSight.cockpit`), treat as secondary pair and label accordingly.

- WeaponPath (primary gun module)
  - Vehicle path: `$.commonWeapons.Weapon[*]`
  - For each weapon entry, fields of interest: `.blk`, `.trigger`, `.triggerGroup`.
  - Selection heuristic (in order):
    1) Candidate set A: weapons where `.trigger` equals `gunner0` or starts with `gunner`, and `.triggerGroup` is not `special`.
    2) For each candidate, open its module (`.blk`) and inspect projectile types. Exclude a candidate if ALL projectiles are rockets/ATGMs (types starting with `atgm`, `rocket`, `he_rocket`, `heat_rocket`).
    3) If multiple remain, prefer the highest `ballisticCaliber` among candidates (read from the module). If ties remain, prefer a module whose projectiles include non-zero `mass` and a non-dummy `bulletName`.
    4) Fallback: if A is empty or ambiguous, include the first weapon whose module lives under `groundModels_weapons` and is not a pure rocket/ATGM module.

- RocketPath(s)
  - Vehicle path: `$.commonWeapons.Weapon[*]`
  - Selection heuristic:
    1) Candidate set R1: weapons where `.triggerGroup == "special"` → collect `.blk`.
    2) Candidate set R2: additionally include weapons whose module projectiles are ALL rockets/ATGMs (types starting with `atgm`, `rocket`, `he_rocket`, `heat_rocket`).
    3) De-duplicate preserving order; keep first two unique paths.

- HasLaser
  - Prefer explicit booleans over substring matches. Check any of:
    - `$.sight.laserRangefinder == true`
    - `$.rangefinder.type == "laser"` or `$.rangefinder.laser == true`
    - `$.crew[*].devices[*].type == "laser_rangefinder"`
    - Known feature flags under `$.modifications` or `$.equipment` that are laser RFs
  - Fallback: if none are present but weapon modules or sights have `laserRangefinder` fields, set true.

### Projectile fields (from module .blkx)

Given a module JSON (from `WeaponPath` or `RocketPath`), parse:

- Name
  - Path: `$.bulletName`
  - Shapes: `string` or `array<string>`
  - Rule: if array, iterate and create one entry per name (or choose the first if you must mirror legacy). Fallback to `bulletType + "/name/short"` if missing.

- Type
  - Path: `$.bulletType` (string) — write raw; normalize later if needed.

- BulletMass
  - Path: `$.mass` (number).

- BallisticCaliber
  - Path: prefer `$.ballisticCaliber`; else `$.caliber` (number).

- Speed
  - Path: `$.speed` (muzzle). If `$.endSpeed` exists, store it separately as `end` but do not overwrite `muzzle`.

- Cx
  - Path: `$.Cx` (`number` or `array<number>`). If array, average numerics; preserve the array in JSON intermediate as `cx.source="array"` and `cx.values` for traceability.

- ExplosiveMass / ExplosiveType
  - Paths: `$.explosiveMass` (number), `$.explosiveType` (string).

- DamageMass / DamageCaliber
  - Paths: `$.damageMass` (number), `$.damageCaliber` (number).

- DeMarre params
  - Paths: `$.demarrePenetrationK`, `$.demarreSpeedPow`, `$.demarreMassPow`, `$.demarreCaliberPow`.
  - Rule: include when present; Stage 2 will default if missing.

- ArmorPower (scalar)
  - Path: `$.armorPower` (number).

- ArmorPower series (distance-indexed)
  - Paths: all properties matching `^ArmorPower(\d+)m$` → collect pairs `{ distance:int, value:number }`, sort by distance.
  - Rule: apply to APDS and APFSDS alike (types starting with `apds` OR `apds_fs`). Legacy .txt keeps APDS* fields; JSON intermediate stores full series.

### Module/vehicle association (presence)

Instead of substring checks, determine presence structurally:

- Read the vehicle’s `$.commonWeapons.Weapon[*]` and their ammo selections if available (some schemas include ammo lists per weapon).
- For a given module, mark `presentInVehicle = true` if its `.blk` equals one of these weapon entries’ `.blk` (or if any of its projectiles appear in the weapon’s ammo listing by name).
- If the schema doesn’t expose ammo arrays, assume `presentInVehicle = true` for modules referenced by the vehicle’s `commonWeapons` entries.

### JSON library and helpers

The Rust implementation uses `serde_json::Value` for flexible querying with pattern matching, replacing the C#/Newtonsoft approach originally described here.

### Selection heuristics (codified)

- IsRocketLike(type): returns true if `type` starts with any of `apfsds_rocket` (rare), `apds_fs`, `atgm`, `rocket`, `he_rocket`, `heat_rocket`.
- SelectPrimaryWeapon(weapons): filter by trigger, exclude rocket-like modules, prefer largest ballisticCaliber, break ties by non-zero mass; fallback to first plausible.
- SelectRocketPaths(weapons): pick `.triggerGroup == "special"` first; then any weapon whose module is rocket-like; dedupe to 2.

### Validation

Golden-diff test suites validate stage 1 output against a committed corpus of expected `Data/{vehicle}.txt` files. Tests are in `tools/fcsgen/tests/stage1.rs` and cover 1168 vehicles.
