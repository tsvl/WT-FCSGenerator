# Sight Families and Options

This document summarizes the supported sight generators, their inputs, key options, and outputs. All sight generators expose a Create(...) method that assembles a .blk script string from ballistic tables and user options.

All families consume:

- `Ballistic/{vehicle}/{shell}.txt` tables for one or more shells
- `Data/{vehicle}.txt` metadata (zoom, HasLaser)
- `Localization/FCS.csv` for UI labels and units

All families output:

- `UserSights/{vehicle}/{sight}.blk` files

Below are the currently implemented families.

## Tochka-SM2 (TochkaAM.cs)

Variants:

- Base — single shell ballistic scale
- DoubleShells — overlays a second shell’s scale (subject to CanUseDoubleShell pairing rules)
- Laser — adds rangefinder/laser behavior and ellipse movement
- Rocket — synthesizes rocket ladder based on constant speed
- Howitzer — adds high-arc ticks and coarse scale

Features:

- Fixed distance tick spacing derived from sensitivity and zoom
- Optional draw of time-of-flight, armor power, and shell labels
- Preemptive lead lines based on speed presets
- Localized labels (`FCS.csv`)

Inputs:

- Ballistic table(s) as text; when Double, the second shell is parsed separately
- Options such as showTime, showAp, inner/outer frame radii, tick density, colors, and language

Outputs:

- `.blk` with drawCircles, drawLines, and drawTexts sections matching the selected options

## Luch (Luch.cs) and Luch Lite (Luch_Lite.cs)

Lightweight reticles focused on clarity and minimalism. They follow the same input/output pattern and provide simpler geometry than Tochka-SM2. Luch Lite further reduces visual density for low-resolution displays.

## Duga (Duga.cs) and Duga-2 (Duga2.cs)

Alternative layout emphasizing different range scales and framing. Duga-2 provides an updated geometry and spacing. Both handle single-shell and optional double-shell overlays.

## Sector (Sector.cs)

Sector-based scale visualization suitable for certain vehicles and playstyles. Supports typical options (time, armor power, labels) and the same localization flow.

## Common options and behavior

- Shell pairing: CanUseDoubleShell(...) enforces sensible pairs (e.g., AP with APHE/APDS; excludes rockets/smoke/practice).
- HE handling: HE shells have zero penetration but are valid for trajectory scales.
- APDS/APFSDS: when ArmorPower arrays are present, the Create(...) method may prefer distance-indexed armor power over DeMarre curves.
- Colors and sizes: All families accept color pickers and numeric sizes from the UI for rings, lines, and fonts.
