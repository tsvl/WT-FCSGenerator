# Stage 3 Reverse-Engineering: `Form1.Button2_Click`

This document captures how stage 3 currently works in `src/Form1.cs` so we can reimplement it cleanly in Rust while preserving behavior where needed.

Scope:

- Stage 3 orchestration in `Button2_Click` (`src/Form1.cs:512`)
- Common parsing/rules used by all sight families
- Variant selection and output naming/path rules
- Parity requirements and known legacy quirks to either preserve or intentionally fix

## 1. Entry flow (what Button2 does now)

1. Validate `comboBox1` sight type is selected (`src/Form1.cs:515`).
2. Resolve `gamePath` from `textBox1` and locate `tools/fcsgen.exe` beside the app (`src/Form1.cs:524`, `src/Form1.cs:527`).
3. Verify `aces.vromfs.bin` exists under `gamePath` (`src/Form1.cs:538`).
4. Run external CLI pipeline:
   - `fcsgen run --game-path ... --output <app_dir> --sensitivity <trackBar1/100>`
   - optional `--ignore-file assets/ignore.txt` if present
   (`src/Form1.cs:563`)
5. If CLI succeeds, stage 3 generation starts using local `Data/` + `Ballistic/`.

Important: stage 3 still uses legacy C# generators (`TochkaSM2.Create`, `Luch.Create`, etc.), only stages 1/2 are delegated to `fcsgen`.

## 2. Inputs consumed by stage 3

From filesystem:

- `Data/{vehicle}.txt` files (`DataPath`) (`src/Form1.cs:20`, `src/Form1.cs:605`)
- `Ballistic/{vehicle}/{shell}.txt` (`BallisticPath`)
- `Datamine/lang.vromfs.bin_u/lang/units_weaponry.csv` (projectile localization)
- `assets/FCS.csv` (Tochka UI/localized text table)

From UI controls:

- Sight family: `comboBox1`
- Language: `comboBox2`
- Output root: `textBox4`
- Variant toggles: `checkedListBox1`
- Nation filter: `checkedListBox2`
- Draw/feature toggles: `checkedListBox3`
- Numeric settings: `trackBar1..6`, `textBox5..10`
- Colors: `pictureBox1..3`

## 3. Core parsing contract for `Data/*.txt`

Per vehicle file parsing pattern:

- Header lines: e.g. `ZoomIn`, `ZoomOut`, `RocketPath`, `HasLaser`.
- Projectile blocks begin at `Name:` and continue until blank line.

Block fields read by stage 3:

- `Type`, `BallisticCaliber`, `Speed`, `ExplosiveMass`, `ExplosiveType`, `ArmorPower`
- For howitzer/internal ballistic path: `BulletMass`, `DamageMass`, `DamageCaliber`, `Cx`, `demarre*`, `APDS*`

APDS array handling:

- Reads lines containing `APDS` and stores up to 12 points in a fixed `[2,12]` array.
- Distance parsed from suffix of the key (e.g. `APDS1000` -> `1000`).

## 4. Shared eligibility/filter rules

Shell is usually processed only if:

- Type is NOT one of: `sam`, `atgm`, `rocket`, `aam`, `smoke`, `shrapnel`, `he`, `practice` (plus `napalm`, `napalm_gel` in some branches),
- OR type is `he` and caliber threshold passes.

HE caliber thresholds are branch-specific in legacy code:

- Common thresholds seen: `>= 0.12`, `>= 0.10`, and `>= 0.037` (double-shell secondary round).

`OnlyRocket` detection:

- Starts `true`, flipped `false` if any projectile type in file is not `atgm|sam|aam`.

Nation filter:

- Vehicle nation inferred from filename prefix (`us_`, `germ_`, `ussr_`, etc.).
- Compared against checked nation labels from `checkedListBox2`.

## 5. Common transforms/helpers

### 5.1 Name normalization for ballistic file lookup

- `BulletNameForBallistic = BulletName.Split('/')[0]`
- If contains `mm_`, drop prefix up to `mm_`, then remove literal `mm_`.
- Used as ballistic filename stem and often output filename component.

### 5.2 `HePenetration(...)`

- Converts explosive mass/type to equivalent HE penetration via:
  - explosive-type multiplier table
  - piecewise interpolation table mass->penetration
- Used when shell type is `he` and direct `ArmorPower` is not sufficient.

### 5.3 `CanUseDoubleShell(...)`

- Encodes allowed/blocked pairings for Tochka double-shell mode.
- Applies type priority groups + explicit exclusions.

### 5.4 Internal `Ballistic(...)`

- Numerical trajectory + penetration calculator (legacy physics).
- Stage 3 uses this mainly in Tochka howitzer branches instead of reading `Ballistic/*.txt`.

## 6. Sight-family dispatch (current structure)

Branches by `comboBox1.Text`:

- `Tochka-SM2`
- `Luch`
- `Luch Lite`
- `Duga`
- `Duga-2`
- `Sector`

All branches iterate vehicle files, parse projectile blocks, load ballistic text if required, and call one `*.Create(...)` method to get `.blk` text.

### 6.1 Tochka-SM2 variants

Enabled by checkbox labels in `checkedListBox1`:

- Base (`Tochka-SM2`)
- Double (`Tochka-SMD2`)
- Laser (`Tochka-SML2`, gated by `HasLaser`)
- Rocket-only (`Tochka-SMR2`, gated by `OnlyRocket`)
- Howitzer (`Tochka-SMH2`, gated by `Speed < 500`)

Tochka language mapping:

- UI language string maps to column index 1..20 in `units_weaponry.csv`.

Tochka filename prefixes:

- `Tochka_SM2_`
- `Tochka_SMD2_`
- `Tochka_SML2_`
- `Tochka_SMR2_`
- `Tochka_SMH2_`

Output naming:

- Optional `ModOptic_` inserted when source file name contains `_ModOptic`.
- Then ballistic shell name(s), and sometimes rocket name.
- Extension `.blk`.

### 6.2 Luch / Luch Lite

- Luch supports rocket pairing path and non-rocket path.
- Luch language handling is only English/Russian in practice.
- Luch/Luch Lite output names are prefixed with `FCS_`.
- Luch Lite excludes napalm rounds explicitly (legacy crash workaround).

### 6.3 Duga / Duga-2 / Sector

- Similar structure: base mode for non-rocket ballistic shells + rocket mode for pure rocket vehicles.
- Use nation filtering and optional draw-feature toggles.
- `Sector` has additional `ForAA` gate (`Type` line must not contain `tank`).

## 7. Output path and file naming contract

Base output dir:

- `<textBox4>/<comboBox1>`

Per vehicle dir:

- `<base>/<vehicle>`
- if source data file contains `_ModOptic`, directory uses vehicle name with `_ModOptic` removed.

File extension:

- `.blk`

Legacy naming is inconsistent across families, but preserving this matters for parity/regression testing.

## 8. UI option-to-runtime mapping

`ComboBox1_SelectedIndexChanged` repopulates checkbox lists and defaults (`src/Form1.cs:3548`):

- Changes visible controls and default coordinate presets per family.
- String labels in checklists are used directly via `.Contains(...)` checks in generation logic.

Implication:

- Option identity is currently label-text based, not stable IDs.
- Renaming labels can silently break generation logic.

## 9. Requirements for Rust stage-3 rewrite (parity target)

Minimum parity requirements:

1. Consume existing `Data/*.txt` and `Ballistic/*/*.txt` formats without requiring migration.
2. Reproduce per-family/variant shell selection filters (including current thresholds) behind compatibility mode.
3. Keep filename/path patterns compatible with legacy defaults.
4. Preserve language lookup behavior (including fallback behavior when CSV columns are missing).
5. Preserve `_ModOptic` handling and nation filtering.
6. Support howitzer path that uses computed ballistic data, or provide equivalent data source strategy with validated output diffs.

Recommended architecture:

1. Parse `Data/*.txt` into typed structs once (avoid repeated `StringReader` scans).
2. Convert checklist label matching to enum/options with stable IDs.
3. Separate:
   - orchestration (vehicle/shell selection)
   - data loading/localization
   - renderer calls per family
4. Add golden tests on generated `.blk` output by family/variant.

## 10. Legacy issues worth tracking (can be fixed behind non-legacy mode)

- Repeated rescans of same text blobs (O(n^2)-ish behavior) across branches.
- String-label-driven logic (`Contains(...)`) is brittle.
- Threshold inconsistencies for HE acceptance (`0.12`, `0.10`, `0.037`).
- Mixed path separators (`\\` and `//`) and Windows-centric assumptions.
- Duplicate language-column mapping blocks repeated many times.
- Some branches rely on side effects from prior parsed state.

## 11. Suggested next doc/work split

Next step after this document:

1. Add per-family deep dives for renderer-specific contracts:
   - `TochkaAM.cs`
   - `Luch.cs` / `Luch_Lite.cs`
   - `Duga.cs` / `Duga2.cs` / `Sector.cs`
2. Define a Rust stage-3 CLI interface (`fcsgen make-sights`) that can run in `--legacy` mode first.
