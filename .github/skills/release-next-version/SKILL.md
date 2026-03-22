---
name: release-next-version
description: >
  Performs a versioned release of cnct-net. Use this when asked to create a release,
  cut a new version, or publish a new package version of cnct.
---

# Release Version

Releases a new version of the cnct NuGet package by following the established release process.

## Repository Context

- **Solution**: `Cnct/Cnct.sln`
- **Package project**: `Cnct/Cnct.NetCore/Cnct.NetCore.csproj` — contains `<BaseVersion>` which is the only version field to update
- **Changelog**: `CHANGELOG.md` at the repo root — tracks unreleased changes and past releases
- **Release branch**: PRs are merged into `release` (not `main` or `develop`)
- **Reference release commit**: `88f22c0384473f7ee08af49cb7e74fef7313cd8e` ("Release 0.3.1")

## Steps

### 1. Evaluate unreleased changes

Read `CHANGELOG.md`. Look at the `## Unreleased` section:
- If it contains `### Feature updates` entries → this is a **minor** release (new functionality)
- If it contains only `### Fixes` entries → this is a **patch** release
- A **major** release (breaking changes) would be explicitly noted; treat as minor otherwise

### 2. Determine the next version

Read the current `<BaseVersion>` from `Cnct/Cnct.NetCore/Cnct.NetCore.csproj`.

Apply semantic versioning (`MAJOR.MINOR.PATCH`):
- **New features** (with or without fixes): increment `MINOR`, reset `PATCH` to 0
- **Fixes only**: increment `PATCH`
- **Breaking changes**: increment `MAJOR`, reset `MINOR` and `PATCH` to 0

Example: current `0.3.1` + new features → `0.4.0`

### 3. Create and switch to the release branch

From the repo root:

```powershell
git checkout develop          # or whichever branch has the unreleased commits
git pull
git checkout -b release-<new-version>   # e.g. release-0.4.0
```

### 4. Update CHANGELOG.md

Transform the `## Unreleased` section:
- **Before** (example):
  ```markdown
  ## Unreleased

  ### Feature updates

  * Add `linkExpand` task ...
  * Add `cloneGitRepository` task ...

  ### Fixes

  * Align version of `Microsoft.PowerShell.SDK` ...
  ```
- **After**:
  ```markdown
  ## Unreleased

  ## 0.4.0

  ### Feature updates

  * Add `linkExpand` task ...
  * Add `cloneGitRepository` task ...

  ### Fixes

  * Align version of `Microsoft.PowerShell.SDK` ...
  ```

Rules:
- Leave `## Unreleased` in place but empty (no content below it before the new version heading)
- Insert the new `## <version>` heading immediately after the blank line following `## Unreleased`
- Do not remove or reorder any existing version sections

### 5. Update the csproj version

In `Cnct/Cnct.NetCore/Cnct.NetCore.csproj`, update the single `<BaseVersion>` element:

```xml
<!-- Before -->
<BaseVersion>0.3.1</BaseVersion>

<!-- After -->
<BaseVersion>0.4.0</BaseVersion>
```

Only `<BaseVersion>` needs changing — `<Version>` is computed from it automatically.

### 6. Commit the changes

Stage both files and commit with the exact message format `Release <version>`:

```powershell
git add CHANGELOG.md Cnct/Cnct.NetCore/Cnct.NetCore.csproj
git commit -m "Release <version>"
# e.g. git commit -m "Release 0.4.0"
```

Do **not** include a `Co-authored-by` trailer in release commits — keep them clean.

### 7. Push the branch

```powershell
git push -u origin release-<new-version>
```

### 8. Create a pull request

Create a PR from `release-<new-version>` → `release` branch:
- **Title**: `Release <version>` (e.g. `Release 0.4.0`)
- **Body**: paste the new version's changelog section (the feature updates and fixes being released)
- **Base branch**: `release` (not `develop` or `main`)

`gh` may not be on the PATH in the current shell session. Always resolve it first:

```powershell
$gh = (Get-Command gh -ErrorAction SilentlyContinue)?.Source ?? "C:\Program Files\GitHub CLI\gh.exe"
```

Run `gh pr create` in **async mode** (sync mode hangs waiting on the TTY). Use backtick-style
newlines (`\`n`) in the body string — do **not** use a here-string or multiline literal, as those
cause the command to hang in sync PowerShell sessions:

```powershell
# async mode — use powershell tool with mode="async", then read output with read_powershell
& $gh pr create --base release --head release-<new-version> --title "Release <version>" --body "## <version>`n`n### Feature updates`n`n* ...`n`n### Fixes`n`n* ..."
```

The command prints the new PR URL on success (e.g. `https://github.com/bgold09/cnct-net/pull/65`).

### 9. Wait for CI on PR 1 (`release-<version>` → `release`)

Stream CI status until all three matrix checks complete. Run in **async mode** and read output
with `read_powershell` — this can take several minutes:

```powershell
# async mode
& $gh pr checks --watch
```

The three required checks are:
- `build (ubuntu-latest)`
- `build (windows-latest)`
- `build (macos-latest)`

If any check fails, **stop** — do not proceed to the merge step. Investigate the failure first.

### 10. Merge PR 1

```powershell
& $gh pr merge --merge
```

This merges `release-<version>` into `release`. The GitHub ruleset enforces the merge method;
do **not** use `--squash` or `--rebase`.

### 11. Create PR 2 (`release` → `main`)

Run in **async mode** and capture the printed PR URL:

```powershell
# async mode
& $gh pr create --base main --head release --title "Release <version>" --body "Release <version>"
```

The command prints the new PR URL (e.g. `https://github.com/bgold09/cnct-net/pull/66`).
Store it as `$pr2`.

### 12. Wait for CI on PR 2

```powershell
# async mode
& $gh pr checks $pr2 --watch
```

Wait for all three `build (ubuntu-latest)` / `build (windows-latest)` / `build (macos-latest)`
checks to pass. Stop and investigate if any fail.

### 13. Merge PR 2

```powershell
& $gh pr merge $pr2 --merge
```

Do **not** use `--squash` or `--rebase`.

### 14. Create PR 3 (`main` → `develop`)

Run in **async mode** and capture the printed PR URL:

```powershell
# async mode
& $gh pr create --base develop --head main --title "Merge main into develop after release <version>" --body "Back-merge main into develop after releasing <version>."
```

Store the returned URL as `$pr3`.

### 15. Wait for CI on PR 3

```powershell
# async mode
& $gh pr checks $pr3 --watch
```

Wait for all three matrix checks to pass. Stop and investigate if any fail.

### 16. Merge PR 3

```powershell
& $gh pr merge $pr3 --merge
```

Do **not** use `--squash` or `--rebase`. This preserves full commit history on `develop`.

## Checklist

- [ ] Unreleased changelog entries identified and change type determined (feature/fix/breaking)
- [ ] New version number calculated using semver
- [ ] Branch `release-<version>` created from latest `develop`
- [ ] `CHANGELOG.md` updated: unreleased items moved under new version heading, `## Unreleased` left empty
- [ ] `<BaseVersion>` in `Cnct/Cnct.NetCore/Cnct.NetCore.csproj` updated
- [ ] Both files staged and committed with message `Release <version>`
- [ ] Branch pushed to origin
- [ ] PR 1 created (`release-<version>` → `release`)
- [ ] PR 1 CI passed (all three matrix checks green)
- [ ] PR 1 merged with merge method
- [ ] PR 2 created (`release` → `main`)
- [ ] PR 2 CI passed (all three matrix checks green)
- [ ] PR 2 merged with merge method
- [ ] PR 3 created (`main` → `develop`)
- [ ] PR 3 CI passed (all three matrix checks green)
- [ ] PR 3 merged with merge method
