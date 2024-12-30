# Windows Laptop Setup Instructions

<!--BEGIN_TOC-->
- [Windows Laptop Setup Instructions](#windows-laptop-setup-instructions)
  - [1. Windows Version](#1-windows-version)
  - [2. .NET Version and Language](#2-net-version-and-language)
  - [3. Dev Drive](#3-dev-drive)
  - [4. General Advice About Winget](#4-general-advice-about-winget)
  - [5. Git](#5-git)
  - [6. 7Zip](#6-7zip)
  - [7. Visual Studio Code](#7-visual-studio-code)
  - [8. Visual Studio](#8-visual-studio)
  - [9. Windows Subsystem for Linux](#9-windows-subsystem-for-linux)
    - [9.1. If You Need To Uninstall WSL](#91-if-you-need-to-uninstall-wsl)
  - [10. Ubuntu Linux](#10-ubuntu-linux)
  - [11. Clone and Build the Code](#11-clone-and-build-the-code)
<!--END_TOC-->

This document has setup instructions for a suggested way of preparing a laptop for doing Mdfmt development under Windows 11.

## 1. Windows Version

Use a recent version of Windows 11.

## 2. .NET Version and Language

Mdfmt is developed using .NET 8.0, using the C# language.  The steps below will get you set up for this.

## 3. Dev Drive

It is suggested to use a Dev Drive, which is a Windows virtual hard drive that is optimized for software development.  See [Set up a Dev Drive on Windows 11](https://learn.microsoft.com/en-us/windows/dev-drive/).

## 4. General Advice About Winget

These instructions include the use of winget.  It is suggested to run winget as administrator to avoid permission problems.

To list the packages that are installed and whether an update is available for each:

```console
winget list
```

To install a new package (not previously installed):

```console
winget install -e --id <packageId>
```

To upgrade a previously installed package:

```console
winget upgrade -e --id <packageId>
```

For example, to search for all packages to install that are named or tagged "aws":

```console
winget search aws
```

## 5. Git

```console
winget install -e --id Git.Git
```

Configure git to handle line endings:

```console
git config --global core.autocrlf input
```

## 6. 7Zip

This will install the `7z` CLI, used to package releases.

```console
winget install -e --id 7zip.7zip
```

For some strange reason, the install does not update the path.  You need to do that manually.  Be sure that `C:\Program Files\7-Zip` is on your path.

Once your path is correct, you should be able to do `7z --help` and get usage information for the `7z` CLI.

## 7. Visual Studio Code

Visual Studio Code with the markdownlint extension is the suggested way for working on documentation.

- Download Visual Studio Code from the [download page](https://code.visualstudio.com/download).
- Run the resulting executable to install the program.
- Accept the license agreement.  Next.
- By default it installs to `$env:USERPROFILE/AppData/Local/Programs/Microsoft VS Code`
- Accept all the defaults, and finish up.

Install the following extensions:

- markdownlint by David Anson

Finally configure settings in Visual Studio Code:

- Use the menu that looks like a gear at the lower left, and select `Settings`.
- In the `Search settings` field type `files.exclude`
  - If it is not there already, add the glob pattern, `**/.github`, to hide the folder with `git` state.  This reduces the chances of accidentally corrupting this data.

## 8. Visual Studio

Visual Studio Community is sufficient for Mdfmt development.  If you have a paid version of Visual Studio, that is fine too.  This section discusses how to install Community.

Go to the [Visual Studio downloads page](https://visualstudio.microsoft.com/downloads/).  Download Visual Studio Community, which downloads `VisualStudioSetup.exe` to your Downloads folder.  Run this executable.

- If it is not already on your machine, it will first install the Visual Studio Installer.
- The Visual Studio Installer will prompt you for what workloads you want to install.  Select the following:
  - ASP.NET and web development
  - .NET desktop development
- Click the `Install` button
- Wait for the installation to finish and reboot when prompted.
- When you run Visual Studio for the first time, make sure you are signed in.  If you are not signed in, there is a `Sign in` link at the top right of the UI.

## 9. Windows Subsystem for Linux

This is needed to run Linux distributions on Windows.  This can be useful if you want to test Mdfmt using Linux.

- In the Start menu, search for `Turn Windows features on or off`.
- Ensure that both the checkboxes for `Virtual Machine Platform` and `Windows Subsystem for Linux` are checked.  Click OK.
- If prompted, reboot the laptop to finish installing the requested changes.
- When the laptop has rebooted, open a PowerShell and run `wsl --update`.  This will ensure that the latest Windows Subsystem for Linux is installed.  If it is already installed, it will ensure that the latest updates have been applied.
- To conrol the amount of memory that Windows Subsystem for Linux can consume, create a file named `.wslconfig` in your `$env:USERPROFILE` directory.  This will keep Rancher Desktop from using too much memory, since it runs in WSL.  For example, to limit memory usage to 6GB, put the following content in `.wslconfig`:

```console
[wsl2]
memory=6GB
```

### 9.1. If You Need To Uninstall WSL

Be careful with this.  This will delete not only WSL, but also any Linux distributions you have installed.

- In the Start menu, search for "Turn Windows features on or off."
- Ensure that both the checkboxes for "Virtual Machine Platform" and "Windows Subsystem for Linux" are unchecked.  Click OK.
- Reboot when prompted.

## 10. Ubuntu Linux

To see if Ubuntu is already installed, do

```console
wsl --list
```

This lists installed Linux distributions.  If Ubuntu is already installed, it will be listed like this:

```console
Ubuntu-24.04
```

If you need to install a Linux distribution:

```console
wsl --list --online
```

This will list available distributions, e.g.:

```console
The following is a list of valid distributions that can be installed.
Install using 'wsl.exe --install <Distro>'.

NAME                            FRIENDLY NAME
Ubuntu                          Ubuntu
Debian                          Debian GNU/Linux
kali-linux                      Kali Linux Rolling
Ubuntu-18.04                    Ubuntu 18.04 LTS
Ubuntu-20.04                    Ubuntu 20.04 LTS
Ubuntu-22.04                    Ubuntu 22.04 LTS
Ubuntu-24.04                    Ubuntu 24.04 LTS
OracleLinux_7_9                 Oracle Linux 7.9
OracleLinux_8_7                 Oracle Linux 8.7
OracleLinux_9_1                 Oracle Linux 9.1
openSUSE-Leap-15.6              openSUSE Leap 15.6
SUSE-Linux-Enterprise-15-SP5    SUSE Linux Enterprise 15 SP5
SUSE-Linux-Enterprise-15-SP6    SUSE Linux Enterprise 15 SP6
openSUSE-Tumbleweed             openSUSE Tumbleweed
```

To install a distribution, for example `Ubuntu-24.04`:

```console
wsl --install -d Ubuntu-24.04
```

When prompted, supply a UNIX user name and password.  Afterwards, you will be in a Linux prompt.  You can now run Linux commands.  When you are done, type `exit` to return to a Windows prompt.

To make a specific distribution the default distribution:

```console
wsl --set-default Ubuntu-24.04
```

Assuming you have set a default distribution, all you have to do to switch to Linux from a Windows prompt is:

```console
wsl
```

To switch to a specific Linux distribution from a Windows prompt:

```console
wsl -d Ubuntu-24.04
```

To switch from Linux back to Windows, just type `exit`.

## 11. Clone and Build the Code

Go to your dev drive (or wherever you want to put the code) and run:

```console
git clone https://github.com/steinar-flatland/mdfmt
```

Then build the code:

```console
cd mdfmt/src
dotnet build
```

The executable is `mdfmt/src/Mdfmt/bin/Debug/net8.0/mdfmt.exe`.

You might want to put this directory on your `PATH`.

Try running

```console
mdfmt --version
```
