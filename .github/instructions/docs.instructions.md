---
applyTo: '**/*.md'
---

# Markdown Style: Literals, Placeholders, Paths

- `literals`: Wrap exact tokens the user types in backticks. Includes commands, subcommands, flags, keys, filenames, extensions, and fixed strings. Example: `fcsgen`, `--emit`, `HasLaser`, `weapon.blkx`.

- `{placeholders}`: Use curly braces for user-supplied values, inside code spans. Never use angle brackets. Common types: `{string}`, `{int}`, `{float}`, `{bool}`, `{path}`, `{glob}`, `{id}`.

- `paths` and files: Keep full paths in code; inject placeholders with braces. Prefer typed placeholders over globs. Examples: `Data/{vehicle}.txt`, `units/tankmodels/{id}.blkx`, `out/{lang}/{name}.json` (prefer this) vs `out/*/*.json` (avoid, unless demonstrating an actual shell glob).

- Command synopsis: Put the whole invocation in one code span. Optional fragments in square brackets, choices with `|`, repeatables with `...`:
  - Example: `tool do --root {path} [--vehicle {id} ...] --mode {legacy|json|both}`

- Defaults, required, constraints: State after the token in plain text parentheses:
  - `--threads {int}` (default: number of logical CPUs)
  - `--datamine-root {path}` (required)

- Flags without values:
  - CLI: `--verbose` (flag; present = true)
  - Schema fields: `HasLaser` — flag; present if true

- Key:value fields: Show literal keys and placeholder values in one code span:
  - `Name:{string}`, `ZoomIn:{float}`, `WeaponPath:{path}`

- Examples: Use fenced code blocks for complete examples; no inline commentary inside the fence. Keep explanatory text before or after the block.

- Angle brackets: Avoid raw `<...>` in prose. If you must mention them, wrap in code (e.g., `<dir>`) or escape as `&lt;dir&gt;`.

- Legend (optional, per doc):
  - `code` = literal token; `{…}` = user-supplied value; `[…]` = optional; `…` = repeatable; choices use `{a|b|c}`.
