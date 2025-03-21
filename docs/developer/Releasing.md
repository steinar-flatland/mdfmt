# Releasing

<!--BEGIN_TOC-->
- [Releasing](#releasing)
  - [Set Up Your Environment](#set-up-your-environment)
  - [Have The Code](#have-the-code)
  - [Version Number](#version-number)
  - [Test The Code](#test-the-code)
  - [Run Create-Release](#run-create-release)
  - [Create Release In GitHub](#create-release-in-github)
<!--END_TOC-->

This document describes a largely manual process for releasing Mdfmt.  This sets the stage for future improvement and automation.

## Set Up Your Environment

(One time only.)

Your laptop should have the following things set in the environment:

1. The environment variable `MDFMT_ROOT` should be set to the directory that contains the Mdfmt code, e.g. `D:\Mdfmt`.
2. The `PATH` environment variable should include `%MDFMT_ROOT%\dev-tools`.

## Have The Code

Have the version of the code you want to release on your laptop.

## Version Number

Be sure that the semantic version number reflects the version number to be released.  This is a `const` in `mdfmt/src/Mdfmt/Program.cs`.

```CSharp
private const string Version = "x.y.z";
```

If necessary, do a pull request to update this.

## Test The Code

Be in the `mdfmt/src` directory and do

```console
dotnet test
```

Be sure all tests pass.

## Run Create-Release

In `mdfmt/dev-tools` is a PowerShell script, `Create-Release.ps1`.  With your `PATH` properly set up, you should be able to run this script from any directory:

```console
Create-Release
```

This script removes the directory `mdfmt/release` if present and recreates it with `.zip` files to be released.  The .zip files are named like this:

```console
mdfmt_x-y-z_linux-x64_framework-net8.0.zip
mdfmt_x-y-z_linux-x64_self-contained.zip
mdfmt_x-y-z_win-x64_framework-net8.0.zip
mdfmt_x-y-z_win-x64_self-contained.zip
```

`x-y-z` will be replaced with the semantic version number.

The `framework-net8.0` releases assume that the machine where the release will be installed has .NET Framework 8.0 installed already.

The `self-contained` releases include the .NET runtime and do not require it to be installed on the target machine already.

## Create Release In GitHub

Note: Wherever `x.y.z` occurs, replace it by the semantic version number you are releasing.

- In GitHub, on the [mdfmt page](https://github.com/steinar-flatland/mdfmt), Click the `Releases` link, and on the next page click the `Draft a new release` button.
- Fill in fields as follows:
  - Choose a tag: `vx.y.z`
  - Target: main
  - Release title: `vx.y.z`
  - Describe this release:  Add release notes.
  - Attach binaries: Drag the binaries (.zip files) generated by `Create-Release` from `mdfmt/release` on the laptop to the `Attach binaries` area in GitHub.
- Once uploading has completed, click the `Publish release` button.
