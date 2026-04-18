# cnct

Install your dotfiles, no matter what platform you're on.

## Build Status

| Branch       |                                                   |
|:-------------|:--------------------------------------------------|
| **develop**  | [![Build status][ci-develop]][ci-develop-history] |
| **main**     | [![Build status][ci-main]][ci-main-history]       |

## Overview

Cnct is a cross-platform command-line tool that aims to make bootstrapping your developer environment
easier. This is accomplished by providing a set of common operations (e.g. creating symlinks) that can
be expressed in a simple configuration.

## Installation

[Create a personal access token (PAT)][create-pat] that has the `read:packages` scope.

```powershell
# When prompted, enter the PAT you created as the password
$c = Get-Credential -UserName "<your GitHub username>"

dotnet nuget add source --name github-bgold09 "https://nuget.pkg.github.com/bgold09/index.json" `
  --username $c.UserName --password $c.GetNetworkCredential().Password

dotnet tool install --global cnct
```

## Usage

The simplest way to run your setup as specified in the configuration file is to run `cnct` from the
directory that contains your `cnct.json` file:

```sh
cd ~/.dotfiles
cnct
```

You can also explicitly point to the location of your config:

```sh
cnct -c ~/.dotfiles/cnct.json
```

### CLI options

| Option | Short | Description |
| :------- | :------ | :------------ |
| `--config <path>` | `-c` | Path to config file. Defaults to `cnct.json` in the current directory. |
| `--quiet` | `-q` | Suppress all output other than errors. |
| `--debug` | `-d` | Output additional debug information. |
| `--validate` | `-v` | Validate the config without executing. |

## Configuration file

The configuration file is how you express the steps that `cnct` should perform. The configuration is an
array of steps that will be completed in order. For the full schema of a `cnct` configuration file, see
the [schema for the cnct version you are using](schema).

The top-level structure of a `cnct.json` file is:

```json
{
  "actions": [
    { "actionType": "...", ... }
  ]
}
```

### Common action properties

Every action supports these properties:

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `actionType` | string | Yes | Identifies the type of action to run. |
| `label` | string | No | Display label shown when the action starts and finishes. |
| `os` | string or array | No | Platform filter: `"windows"`, `"linux"`, `"osx"`. Omit to run on all. |
| `tags` | string or array | No | Tags filter. See [Machine settings](#machine-settings). |

### Action types

#### `link` — Create symlinks

Creates one or more symbolic links from source files or directories to destination paths.

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `links` | object | Yes | Map of source path → destination path(s). |

Each value in `links` can be:

* A **string** — a single destination path.
* An **array of strings** — multiple destination paths.
* A **platform object** with keys `windows`, `linux`, `osx`, and/or `unix`, each pointing to a
  string or array of destination paths. This lets you use different destinations per platform.

**Example:**

```json
{
  "actionType": "link",
  "links": {
    "gitconfig": "~/.gitconfig",
    "nvim/init.vim": {
      "unix": "~/.config/nvim/init.vim",
      "windows": "~/AppData/Local/nvim/init.vim"
    }
  }
}
```

---

#### `linkExpand` — Expand a directory into individual symlinks

Creates a symlink in a target directory for each subdirectory found inside a source directory.
Useful for linking batches of config directories (e.g. XDG config folders) without listing each one.

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `source` | string | Yes | Directory whose subdirectories will be symlinked. |
| `target` | string | Yes | Directory in which to create the symlinks. |

**Example:**

```json
{
  "actionType": "linkExpand",
  "source": "config",
  "target": "~/.config"
}
```

---

#### `copy` — Copy files

Copies source files or directories to destination paths. The `files` map has the same shape as the
`links` map in the `link` action.

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `files` | object | Yes | Map of source path → destination path(s). |

**Example:**

```json
{
  "actionType": "copy",
  "files": {
    "ssh/config": "~/.ssh/config"
  }
}
```

---

#### `shell` — Run a shell command

Invokes a command in a shell. On Windows you can use `powershell`; on Linux and macOS you can use
`sh`.

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `command` | string | Yes | The command to invoke. |
| `shell` | string | Yes | Shell to use: `"powershell"` or `"sh"`. |
| `silent` | boolean | No | If `true`, suppress the command's output. Default: `false`. |

**Example:**

```json
{
  "actionType": "shell",
  "shell": "sh",
  "command": "chmod 600 ~/.ssh/config",
  "os": "linux"
}
```

---

#### `environmentVariable` — Set an environment variable

Sets a persistent user-scoped environment variable.

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `name` | string | Yes | Name of the environment variable. |
| `value` | string | Yes | Value to assign. |

**Example:**

```json
{
  "actionType": "environmentVariable",
  "name": "EDITOR",
  "value": "vim"
}
```

---

#### `cloneGitRepository` — Clone or update git repositories

Clones a git repository to a local path. If the destination already exists, performs a `git pull`
instead.

| Property | Type | Required | Description |
| :--------- | :----- | :--------- | :------------ |
| `repos` | object | Yes | Map of repository URL → local destination path. |

**Example:**

```json
{
  "actionType": "cloneGitRepository",
  "repos": {
    "https://github.com/tmux-plugins/tpm": "~/.tmux/plugins/tpm"
  }
}
```

---

## Machine settings

You can restrict individual actions to run only on machines with specific tags. Tags are defined in
a `settings.json` file on each machine:

| Platform | Path |
| :--------- | :----- |
| Windows | `%LOCALAPPDATA%\cnct\settings.json` |
| Linux / macOS | `$XDG_CONFIG_HOME/cnct/settings.json` or `~/.config/cnct/settings.json` |

**Example `settings.json`:**

```json
{
  "tags": ["work", "linux-desktop"]
}
```

An action that declares `"tags"` runs only when the machine's `settings.json` includes at least one
matching tag (case-insensitive). Actions without a `tags` property always run.

**Example action with tags:**

```json
{
  "actionType": "shell",
  "shell": "sh",
  "command": "sudo apt-get install -y build-essential",
  "tags": "linux-desktop"
}
```

---

## Full example

Below is a `cnct.json` that demonstrates every action type:

```json
{
  "$schema": "https://raw.githubusercontent.com/bgold09/cnct-net/main/schema/v0.6.0/cnctConfig.json",
  "actions": [
    {
      "actionType": "environmentVariable",
      "name": "EDITOR",
      "value": "vim"
    },
    {
      "actionType": "link",
      "label": "Link dotfiles",
      "links": {
        "gitconfig": "~/.gitconfig",
        "nvim/init.vim": {
          "unix": "~/.config/nvim/init.vim",
          "windows": "~/AppData/Local/nvim/init.vim"
        }
      }
    },
    {
      "actionType": "linkExpand",
      "label": "Link XDG config dirs",
      "source": "config",
      "target": "~/.config",
      "os": ["linux", "osx"]
    },
    {
      "actionType": "copy",
      "label": "Copy SSH config",
      "files": {
        "ssh/config": "~/.ssh/config"
      }
    },
    {
      "actionType": "cloneGitRepository",
      "label": "Clone tmux plugin manager",
      "repos": {
        "https://github.com/tmux-plugins/tpm": "~/.tmux/plugins/tpm"
      },
      "os": ["linux", "osx"]
    },
    {
      "actionType": "shell",
      "shell": "sh",
      "command": "chmod 600 ~/.ssh/config",
      "os": ["linux", "osx"],
      "tags": "work"
    },
    {
      "actionType": "shell",
      "shell": "powershell",
      "command": "Set-ExecutionPolicy RemoteSigned -Scope CurrentUser",
      "os": "windows"
    }
  ]
}
```

## Thanks and Credit for Inspiration

* [Anish Athalye](https://github.com/anishathalye) for [dotbot](https://github.com/anishathalye/dotbot),
  which heavily inspired this project

[create-pat]: https://docs.github.com/en/github/authenticating-to-github/keeping-your-account-and-data-secure/creating-a-personal-access-token
[ci-develop]: https://github.com/bgold09/cnct-net/actions/workflows/actions-main.yml/badge.svg?branch=develop
[ci-main]: https://github.com/bgold09/cnct-net/actions/workflows/actions-main.yml/badge.svg?branch=main
[ci-develop-history]: https://github.com/bgold09/cnct-net/actions?query=event%3Apush+branch%3Adevelop
[ci-main-history]: https://github.com/bgold09/cnct-net/actions?query=event%3Apush+branch%3Amain
