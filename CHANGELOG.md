# Changelog

## Unreleased

## 0.6.0

### Feature updates

* The `os` property is now supported on all action types. When set, the action
  only runs on the specified operating system(s).
  ([#78](https://github.com/bgold09/cnct-net/pull/78))
* Add `sh` shell type for running commands via `/bin/sh` on Linux and macOS.
  The `os` property on shell actions is now optional — when omitted, the action
  runs on all platforms.
  ([#78](https://github.com/bgold09/cnct-net/pull/78))
* Each action now logs a start (`>`) and finish (`✓`) event. An optional
  `"label"` property can be added to actions in `cnct.json` to add context to
  the display.
  ([#79](https://github.com/bgold09/cnct-net/pull/79))
* Add `--validate` / `-v` option to validate a `cnct.json` config file without
  executing it. Results are written to stdout as JSON
  (`{ "valid": bool, "issues": [...] }`) so the output can be consumed by
  scripts and CI/CD pipelines. Exit code is `0` when the config is valid and
  `1` when there are errors. Validation checks include:
  * `link` / `copy`: each source file or directory must exist on disk.
  * `linkExpand`: the source directory must exist on disk.
  * `cloneGitRepository`: each repository URL must be a valid absolute URI.
  ([#84](https://github.com/bgold09/cnct-net/pull/84))

## 0.5.0

### Feature updates

* Add support for machine-specific task filtering via tags. Each action in `cnct.json` can
  optionally declare a `"tags"` property (a string or array of strings). A machine's tags
  are defined in a `settings.json` file in the platform local config directory
  (`%LOCALAPPDATA%\cnct\settings.json` on Windows;
  `$XDG_CONFIG_HOME/cnct/settings.json` or `~/.config/cnct/settings.json` on Linux and
  macOS). An action runs if it has no tags, or if the machine's tags include at least one
  match (case-insensitive).
  ([#72](https://github.com/bgold09/cnct-net/pull/72))

### Improvements

* Migrate all tasks to use `IFileSystem` from `System.IO.Abstractions` for filesystem
  operations, enabling full unit test coverage without real disk I/O.
  ([#70](https://github.com/bgold09/cnct-net/pull/70))

## 0.4.0

### Feature updates

* Add `linkExpand` task which creates individual symlinks for each subdirectory of a source
  directory into a target directory.
* Add `cloneGitRepository` task which clones git repositories and keeps them up to date.

### Fixes

* Align version of `Microsoft.PowerShell.SDK` with `System.Management.Automation`

## 0.3.1

### Feature updates

* Publish package to nuget.org.

## 0.3.0

### Feature updates

* Add `copy` task which copies source files on the machine.
* Update to .NET 8 runtime and update packages.

## 0.2.1

### Fixes

* Create link destination directories if they do not exist.

### Feature updates

* Add task to set environment variables for the executing user.

## 0.2.0

### Feature updates

* Update to .NET 6 runtime and update packages (#27)
* Create UNIX-specific symlinks (#26)

### Fixes

* Fix symlink creation on linux (#24)

## 0.1.0

Initial release of cnct.
