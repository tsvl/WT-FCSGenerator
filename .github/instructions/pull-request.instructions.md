---
applyTo: '**/.github/PULL_REQUEST_TEMPLATE*.md'
---

# Pull Request Description Style

- Title: Conventional Commit format — `{type}[({scope})]!: {description}`
  - Match the main change; keep concise and imperative

- Summary (1–3 sentences):
  - What changed and why (problem/motivation)

- Breaking changes (if any):
  - Describe impact and migration steps

- Links:
  - `Closes #{id}` / `Fixes #{id}` for issues; related discussions/PRs

- Checklist:
  - CI passes
  - Docs updated (`README.md`/`docs/`) if user-facing
  - `CHANGELOG.md` updated under `## [Unreleased]`
  - Small, focused scope (split if large or unrelated)
