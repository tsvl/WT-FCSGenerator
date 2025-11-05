# Contributing to WT-FCSGenerator

Thanks for your interest in contributing! This document outlines the development workflow and conventions for this project.

## Quick Start

1. **Fork and clone** the repository
2. **Install .NET 10 SDK** (preview): [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
3. **Build the project:**

   ```powershell
   cd src
   dotnet build
   ```

4. **Run the application:**

   ```powershell
   dotnet run
   ```

## Branching Strategy

We use **GitHub Flow** - a simple, streamlined workflow:

- **`main`** - Always stable and deployable. All releases are tagged from here.
- **Feature branches** - Short-lived branches for new features, fixes, or improvements.

### Workflow

1. **Create a branch** from `main`:

   ```powershell
   git checkout main
   git pull origin main
   git checkout -b feat/your-feature-name
   # or
   git checkout -b fix/your-bugfix-name
   ```

2. **Make your changes** and commit regularly using [Conventional Commits](#commit-conventions)

3. **Push your branch** and open a Pull Request to `main`

4. **After merge**, the branch is deleted and you can pull the updated `main`

### Branch Naming

Use descriptive names with prefixes:

- `feat/` - New features (e.g., `feat/extract-datamine-runtime`)
- `fix/` - Bug fixes (e.g., `fix/sight-generation-crash`)
- `docs/` - Documentation only (e.g., `docs/update-readme`)
- `refactor/` - Code refactoring (e.g., `refactor/cleanup-ballistics`)
- `chore/` - Maintenance tasks (e.g., `chore/update-dependencies`)

### Historical Note

The `dev` branch exists from the v2.0.0 cleanup effort and may be used in the future for major long-running refactors. For normal development, **always branch from `main`**.

## Commit Conventions

We use [Conventional Commits](https://www.conventionalcommits.org/) for clear, structured commit messages:

```
{type}[({scope})]: {description}

{body}

{footer(s)}
```

### Commit Types

- **feat:** New feature
- **fix:** Bug fix
- **docs:** Documentation changes
- **refactor:** Code refactoring (no behavior change)
- **perf:** Performance improvements
- **test:** Adding or updating tests
- **chore:** Maintenance tasks (dependencies, configs, etc.)
- **ci:** CI/CD changes

### Examples

```
feat: add runtime datamine extraction

Integrates wt_ext_cli to extract data directly from War Thunder
install without manual PowerShell script invocation.

Closes #42
```

```
fix: prevent crash when UserSights folder is missing

Creates the directory automatically if it doesn't exist instead
of throwing an exception.
```

```
chore: upgrade to .NET 10 RTM

Updates from preview to stable release.
```

### Breaking Changes

For breaking changes, add `!` after the type or include `BREAKING CHANGE:` in the footer:

```
feat!: change sight output format to JSON

BREAKING CHANGE: Sight files are now JSON instead of plain text.
Users will need to regenerate all sights.
```

## Pull Request Process

1. **Ensure CI passes** - GitHub Actions will build and test your changes
2. **Keep PRs focused** - One feature/fix per PR when possible
3. **Update documentation** - If your changes affect user-facing behavior, update README.md
4. **Update CHANGELOG.md** - Add your changes under `## [Unreleased]`
5. **Self-review** - Even if you have write access, open a PR for review and discussion

### PR Title Format

Use Conventional Commit format for PR titles:

- `feat: add sight type XYZ`
- `fix: resolve crash on invalid input`
- `docs: improve installation instructions`

## Code Style

- **C#**: Follow standard .NET conventions
  - PascalCase for public members
  - camelCase for private fields (prefixed with `_`)
  - Use `var` when type is obvious
  - Prefer explicit type when it aids clarity

- **PowerShell**: Follow [PowerShell best practices](https://poshcode.gitbook.io/powershell-practice-and-style/)
  - Use approved verbs (Get-, Set-, New-, etc.)
  - PascalCase for function names
  - Full cmdlet names in scripts (avoid aliases)

- **Line endings**: LF (Unix-style) - handled by `.gitattributes`
- **Indentation**: Tabs for C#, spaces for config files (see `.editorconfig`)

## Project Structure

```
WT-FCSGenerator/
├── .github/          # GitHub Actions workflows, templates, etc.
├── assets/           # Pre-generated data files (transitional, to be removed)
├── src/              # C# source code
│   ├── FCS.csproj   # Project file
│   ├── Program.cs   # Entry point
│   └── *.cs         # Application code
└── README.md         # User-facing documentation
```

## Testing

Currently, the project lacks automated tests (inherited technical debt). When making changes:

1. **Manual testing** - Run the application and verify your changes work
2. **Test edge cases** - Try invalid inputs, missing files, etc.
3. **Test the build** - Ensure `dotnet publish` produces a working executable

**Future goal:** Add unit tests for core logic (ballistics calculations, file generation, etc.)

## Release Process

Releases are automated via GitHub Actions:

1. **Merge changes** to `main`
2. **Update CHANGELOG.md** - Move items from `[Unreleased]` to a new version section
3. **Create and push a tag**:

   ```powershell
   git tag -a v2.1.0 -m "Release v2.1.0"
   git push origin v2.1.0
   ```

4. **GitHub Actions** builds, packages, and creates a release automatically

Tags must follow the format `vX.Y.Z` (e.g., `v2.0.0`, `v2.1.3`).

## Getting Help

- **Issues**: Open an issue for bugs, feature requests, or questions
- **Discussions**: Use GitHub Discussions for general questions or ideas
- **Discord**: Join the [Discord server](https://discord.gg/XrTMMQ6R) for real-time help

## Code of Conduct

Be respectful and constructive. We're all here to make this tool better for the War Thunder community.

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (to be determined - currently pending clarification from original author).
