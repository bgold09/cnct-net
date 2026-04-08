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

## Building and testing

Build the solution with:

```
dotnet build Cnct/Cnct.sln
```

Run all tests with:

```
dotnet test Cnct/Cnct.sln
```

**After every change, verify that the build produces zero warnings and zero errors.**
StyleCop warnings are treated as warnings (not errors) but must still be fixed — do not
leave any `warning SA*` or `warning CA*` in the build output.

## JSON schema

`schema/cnctConfig.vnext.json` must be kept in sync with the code. Each new task type needs:
1. A `$ref` added to `actions.items.oneOf`
2. A new `definitions/<Name>Action` block with `allOf: [ActionBase, { required, properties }]`

## Code style

- **Maximum line length is 120 characters** for all C# source files.
- **One class per file.** Do not place multiple classes in the same file.

### StyleCop rules

StyleCop.Analyzers (v1.1.118) is enabled on all projects. Configuration
lives in `Cnct/.editorconfig`. Key active rules to follow:

- **SA1204**: Static members must appear before non-static members.
- **SA1202**: `public` members must appear before `protected` members, which must appear
  before `private` members. When adding a `protected` override (e.g. `GetAdditionalDisplayText()`),
  place it **after** all `public` members in the class.
- **SA1101**: Prefix local calls with `this.`.
- **SA1309**: Field names must not begin with underscore.
- **SA1128**: Put constructor initializers on their own line.
- **SA1502**: Element should not be on a single line.
- **SA1513**: Closing brace must be followed by a blank line.
- **SA1516**: Elements (methods, properties) must be separated by a blank line.

The following rules are **suppressed** (severity = none):

- SA0001 (XML comment analysis disabled)
- SA1200 (using directive placement — configured to outside namespace)
- SA1201 (element ordering — disabled)
- SA1600, SA1601, SA1602 (XML documentation not required)
- SA1633 (file header not required)
- CA1303 (localized parameters not required)
- CA2007 (ConfigureAwait not required)
- CA1031 (catching general exceptions allowed)
- CA1819 (array properties allowed)

## Markdown

All markdown files must comply with `.markdownlint.json`:

- **Line length** — maximum **104 characters** (code blocks are exempt)
- **no-duplicate-heading** — disabled (duplicate headings are allowed)
- **ul-start-left** — disabled
- Wrap continuation lines with 2-space indent to stay within the limit

## Changelog

`CHANGELOG.md` — new features go under `## Unreleased → ### Feature updates`, fixes under
`### Fixes`.
