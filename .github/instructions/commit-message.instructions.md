---
applyTo: '**/COMMIT_EDITMSG'
---

# Commit Message Style: Conventional Commits

- Format: `{type}[({scope})]!: {description}`
  - `type`: `feat|fix|docs|refactor|perf|test|chore|ci`
  - `scope` (optional): component or area, short and lowercase
  - `!` indicates a breaking change (alternatively use a `BREAKING CHANGE:` footer)

- Subject line:
  - Imperative mood, concise, no trailing period
  - Aim for ≤ 72 characters

- Body (optional but recommended):
  - Explain the what and why, not just the how
  - Wrap at ~72 chars; bullets allowed

- Footers:
  - Link issues with `Closes #{id}` / `Fixes #{id}`
  - Breaking changes: `BREAKING CHANGE: {impact and migration notes}`
  - Other trailers supported (e.g., `Co-authored-by:`)

- Types:
  - `feat`: new feature
  - `fix`: bug fix
  - `docs`: documentation only
  - `refactor`: code change without behavior change
  - `perf`: performance improvement
  - `test`: add/update tests
  - `chore`: maintenance (deps, configs)
  - `ci`: CI/CD changes

- Examples:
  - `feat: add runtime datamine extraction`
  - `fix: prevent crash when UserSights folder is missing`
  - `chore!: drop .NET 8 support\n\nBREAKING CHANGE: Requires .NET 10.`
