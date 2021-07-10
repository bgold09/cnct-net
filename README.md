# cnct

Install your dotfiles, no matter what platform you're on.

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

dotnet tool install --global cnct --source github-bgold09 
```

## Configuration file

The configuration file is how you express the steps that `cnct` should perform. The configuration is an
array of steps that will be completed in order. For the full schema of a `cnct` configuration file, see
the [schema for the cnct version you are using](schema).

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

## Thanks and Credit for Inspiration

* [Anish Athalye](https://github.com/anishathalye) for [dotbot](https://github.com/anishathalye/dotbot),
  which heavily inspired this project

[create-pat]: https://docs.github.com/en/github/authenticating-to-github/keeping-your-account-and-data-secure/creating-a-personal-access-token