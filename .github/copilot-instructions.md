# Copilot Instructions for cnct-net

## What this project is

**cnct-net** is a cross-platform bootstrapping tool written in C# (.NET 8). It reads a `cnct.json` config file and runs a sequence of declarative *actions* (creating symlinks, copying files, running shell commands, cloning git repos, setting environment variables). The primary consumer is a personal `.dotfiles` repository.

## Solution structure

```
Cnct/
├── Cnct.Core/           # Core library — all task types live here
├── Cnct.Core.Tests/     # xUnit test project
├── Cnct.NetCore/        # CLI entry point (thin wrapper over Cnct.Core)
└── Cnct.SourceGeneration/  # Roslyn source generator (auto-registers task types)
```

## How task types work

Each task type requires **two** files in `Cnct.Core`:

1. **`Configuration/<Name>TaskSpecification.cs`** — JSON-deserializable config class, decorated
   with `[CnctActionType("<actionType>")]`. Extends `CnctActionSpecBase` (which implements
   `ICnctActionSpec`) and overrides `Validate()` + `ExecuteAsync(ILogger, configDirectoryRoot)`.
   Must be `public sealed partial` — the source generator emits the `ActionType` property and
   wires it into `CnctActionConverter`.

2. **`Tasks/<Name>Task.cs`** — `internal` class extending `CnctTaskBase`, does the actual work. Created and called by the specification's `ExecuteAsync`.

The source generator (`CnctTaskSpecificationGenerator`) scans for classes decorated with
`[CnctActionType]` and generates:
- A partial `override ActionType` property on the spec class
- A `switch` arm in `CnctActionConverter.GetActionSpecFromType()` to deserialize it

**No manual registration is needed** — adding the attribute is sufficient.

## Existing task types

| `actionType`          | Specification class                          | Task class                     | What it does |
|-----------------------|----------------------------------------------|--------------------------------|--------------|
| `link`                | `LinkTaskSpecification`                      | `LinkTask`                     | Creates symlinks (1:1, with platform-specific destinations) |
| `linkExpand`          | `LinkExpandTaskSpecification`                | `LinkExpandTask`               | Creates individual symlinks for each subdirectory of a source dir into a target dir |
| `copy`                | `CopyTaskSpecification`                      | `CopyTask`                     | Copies files |
| `shell`               | `ShellTaskSpecification`                     | `PowerShellInvoker`            | Runs a PowerShell command |
| `environmentVariable` | `EnvironmentVariableTaskSpecification`       | *(inline)*                     | Sets a user environment variable |
| `cloneGitRepository`  | `CloneGitRepositoryTaskSpecification`        | `CloneGitRepositoryTask`       | Clones a git repo, or pulls if already present |

## Key conventions

### Path handling
- All paths go through `PathExtensions.NormalizePath()`: expands `~` to the platform home directory and normalises directory separators.
- `~`-prefixed and absolute paths are used as-is. **Relative paths must be resolved against `configDirectoryRoot`** (the directory containing `cnct.json`) using `Path.IsPathRooted` + `Path.Combine`. Both `LinkExpandTaskSpecification` and `CloneGitRepositoryTaskSpecification` do this — follow the same pattern for any new spec that takes a path.

### Platform
- `Platform.CurrentPlatform` returns `PlatformType.Windows | Linux | OSX`.
- `Platform.CurrentPlatformIsUnix` covers Linux + OSX.
- Symlink creation uses `NativeMethods`: `CreateSymbolicLink` (Win32) or `CreateLinuxSymlink` (libc).

### Dependency injection pattern
When a task needs an external collaborator (filesystem, shell, git), inject it behind an interface with a default production implementation:
- `CopyTask` takes `IFileSystem` (defaults to `new FileSystem()`)
- `CloneGitRepositoryTask` takes `IGitRunner` (defaults to `new ProcessGitRunner()`)
- `LinkTaskSpecification`/`CopyTaskSpecification` take `IFileManagement` (defaults to `new FileManagement()`)

The spec class should expose a constructor accepting the interface so tests can inject mocks. See `CloneGitRepositoryTaskSpecification(IGitRunner)` as the reference.

### IGitRunner
`IGitRunner` has two semantic methods:
- `CloneAsync(string url, string destination)`
- `PullAsync(string repositoryPath)`

`ProcessGitRunner(ILogger logger)` is the production implementation; it routes all git output through `ILogger` (not `Console`), reads stdout/stderr **concurrently** (`Task.WhenAll`) to prevent deadlock, and throws on non-zero exit code. The logger is provided at construction time — in the default code path `CloneGitRepositoryTaskSpecification` creates it lazily in `ExecuteAsync` (when the logger is available): `this.gitRunner ?? new ProcessGitRunner(logger)`.

### Testing patterns
- Deserialization: `JsonConvert.DeserializeObject<ICnctActionSpec>(json)` — asserts the concrete type and checks key properties.
- Validation: construct the spec directly, call `Validate()`, assert `InvalidOperationException`.
- `ExecuteAsync` behaviour: construct the spec with a mocked `IGitRunner`/`IFileSystem`/`IFileManagement`, set up real temp-directory filesystem state where needed, verify mock calls with `Moq`.
- Specs that check filesystem state (clone vs pull, link exists vs not) set up and tear down temp directories in `try/finally`.

## JSON schema

`schema/cnctConfig.vnext.json` must be kept in sync with the code. Each new task type needs:
1. A `$ref` added to `actions.items.oneOf`
2. A new `definitions/<Name>Action` block with `allOf: [ActionBase, { required, properties }]`

## Code style

- **Maximum line length is 120 characters** for all C# source files.

## Changelog

`CHANGELOG.md` — new features go under `## Unreleased → ### Feature updates`, fixes under
`### Fixes`. All edits must comply with `.markdownlint.json`:

- Maximum line length **104 characters** (code blocks are exempt)
- Wrap continuation lines with 2-space indent to stay within the limit
