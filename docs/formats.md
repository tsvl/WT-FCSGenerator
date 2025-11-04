# Intermediate Formats

This document specifies the file formats used between the pipeline stages. These formats are plain text and stable enough to support external tooling or CLI rewrites.

## Data/{vehicle}.txt (stage 1 output)

Text file with simple key:value pairs and repeated projectile blocks. Example header lines:

- `WeaponPath:{relative/path/to/weapon.blkx}`
- `RocketPath:{relative/path/to/rocket.blkx}` (optional)
- `ZoomIn:{float}`
- `ZoomOut:{float}`
- `HasLaser` (flag line without a value; present if the vehicle has a laser)

Then one or more projectile entries, each starting with Name:{id}. Fields are one per line; unknown or unused fields may be omitted. Example fields include:

- `Name:{string}` — projectile identifier used for filenames and sight labels
- `Type:{string}` — projectile type (e.g., he_frag_i, ap_t, apds_autocannon, apds_fs_long_tank, atgm_tandem_tank, atgm_vt_fuze_tank)
- `BulletMass:{float}` — projectile mass in kg
- `BallisticCaliber:{float}` — ballistic caliber in meters (e.g., 0.03 for 30 mm)
- `Speed:{float}` — muzzle velocity in m/s
- `Cx:{float|list}` — drag coefficient. If a list is found in datamine, stage 1 writes an averaged value.
- `ExplosiveMass:{float}` — mass of explosive filler in kg (HE, HEI, etc.)
- `ExplosiveType:{string}` — explosive type key (e.g., a_ix_2, ocfol)
- `demarrePenetrationK:{float}` — DeMarre base coefficient
- `demarreSpeedPow:{float}`
- `demarreMassPow:{float}`
- `demarreCaliberPow:{float}`
- `DamageMass:{float}` — for composite rounds
- `DamageCaliber:{float}`
- `ArmorPower:{float}` — for rockets/ATGMs or APDS-FS when a single representative value is used

APDS-FS armor power arrays (if present in the source) are flattened into scalar fields named for range breakpoints, e.g.:

- APDS0, APDS100, APDS200, ..., APDS10000 — armor power values (mm) indexed by distance in meters

Blocks are separated by a blank line. Example (truncated, from Data/ussr_bmp_2m.txt):

```data
Name:30mm_UOF8
Type:he_frag_i
BulletMass:0.389
BallisticCaliber:0.03
Speed:960.0
Cx:0.29841
ExplosiveMass:0.049
ExplosiveType:a_ix_2
demarrePenetrationK:0.15
demarreSpeedPow:1.43
demarreMassPow:0.71
demarreCaliberPow:1.07
```

Note:

- For exact extraction rules (which keys are read from the datamine, how arrays vs scalars are handled, and all defaults/fallbacks), see docs/datamine-to-data.md.
- APDS-FS armor power series may be omitted in current Stage 1 because only types starting with `apds` trigger series capture. This will be addressed in a future rewrite.

## Ballistic/{vehicle}/{shell}.txt (stage 2 output)

Tabular file with three columns separated by tabs:

`{distance_m}	{time_s}	{penetration_mm}`

- `distance_m`: floating-point distance along the line of fire in meters
- `time_s`: time of flight to that distance in seconds
- `penetration_mm`: integer or float penetration in mm (0 for HE and non-penetrating munitions)

Rows start at 0 distance and increase monotonically. Example (truncated, from Ballistic/ussr_bmp_2m/UBR6.txt):

```tsv
0.000	0	65
121.166	0.1	62
246.138	0.3	59
365.461	0.4	56
479.653	0.5	54
589.051	0.7	51
...
```

## Localization CSVs

Sight rendering uses localized labels loaded from CSV files in Localization/:

- FCS.csv — UI labels for the sights (e.g., Rangefinder, Target lock, ON, Distance, units)
- units_weaponry.csv — human-readable weapon names used when resolving projectile/sight names

These CSVs have a first column key and one column per language (English, French, German, Russian, etc.). The Create(...) methods select language-specific strings based on the app’s UI setting.

## UserSights/{vehicle}/{sight}.blk (stage 3 output)

Generated War Thunder sight scripts. Their contents depend on sight family, language, and options. At a high level, they include drawing sections like:

- `drawCircles` — reticle rings, central dot, frames
- `drawLines` — axis lines, preemptive lead lines, distance correction ticks
- `drawTexts` — labels such as sight name, range scale, time/armor power readouts

Each sight family has its own layout conventions. See docs/sights.md for family-specific notes.
