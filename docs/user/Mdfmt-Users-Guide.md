# Mdfmt User's Guide

<!--BEGIN_TOC-->
- [Mdfmt User's Guide](#mdfmt-users-guide)
  - [1. Introduction](#1-introduction)
  - [2. Getting Started](#2-getting-started)
    - [2.1. Downloading And Installing](#21-downloading-and-installing)
    - [2.2. Checking The Version](#22-checking-the-version)
    - [2.3. Displaying Help](#23-displaying-help)
  - [3. md File Extension](#3-md-file-extension)
  - [4. Usage Overview](#4-usage-overview)
  - [5. Options And Target Path](#5-options-and-target-path)
    - [5.1. Traversal Options](#51-traversal-options)
      - [5.1.1. Recursive](#511-recursive)
    - [5.2. Informational Options](#52-informational-options)
      - [5.2.1. Verbose](#521-verbose)
      - [5.2.2. Help](#522-help)
      - [5.2.3. Version](#523-version)
    - [5.3. Environment](#53-environment)
    - [5.4. Formatting Options](#54-formatting-options)
      - [5.4.1. Flavor](#541-flavor)
      - [5.4.2. Heading Numbers](#542-heading-numbers)
      - [5.4.3. TOC Threshold](#543-toc-threshold)
      - [5.4.4. Line Numbering Threshold](#544-line-numbering-threshold)
      - [5.4.5. Newline Strategy](#545-newline-strategy)
    - [5.5. Target Path](#55-target-path)
  - [6. Output](#6-output)
    - [6.1. Non-Verbose Output](#61-non-verbose-output)
    - [6.2. Verbose Output](#62-verbose-output)
  - [7. Configuration](#7-configuration)
    - [7.1. Configuration File Names](#71-configuration-file-names)
    - [7.2. How Configuration Files Are Located](#72-how-configuration-files-are-located)
    - [7.3. Configuration File Format](#73-configuration-file-format)
    - [7.4. Loading Multiple Configuration Files](#74-loading-multiple-configuration-files)
    - [7.5. Options Inheritance](#75-options-inheritance)
      - [7.5.1. Example: Deciding How To Format The Glossary](#751-example-deciding-how-to-format-the-glossary)
  - [8. Return Value](#8-return-value)
<!--END_TOC-->

## 1. Introduction

Mdfmt is a command line interface (CLI) for Markdown formatting.  Able to operate on individual Markdown files and on directory structures containing many Markdown files, it automates maintenance of heading numbers, in-document links, cross-document links, table of contents, line numbering of fenced code blocks, consistent newlines, etc.  Mdfmt is designed to help with both simple use cases where only one kind of formatting is required, and with more complex use cases involving multiple target Markdown rendering environments with different formatting needs.

See also this [Glossary](./Glossary.md), which defines the language used to talk about Mdfmt.

## 2. Getting Started

### 2.1. Downloading And Installing

A version of Mdfmt is released as a collection of `.zip` files, each `.zip` file targetting a different combination of _runtime_ and _deployment type_.

- _Runtime_ indicates an operating system and processor architecture.
- _Deployment type_ is either _self-contained_ or _framework-dependent_, with respect to a specific version of .NET.
  - _Self-contained_ means that the release includes the .NET runtime.
  - _Framework-dependent_ means that the release does not include the .NET runtime, so the runtime needs to be installed separately.

Releases can be downloaded from the [releases](https://github.com/steinar-flatland/mdfmt/releases) page.  Within a release, each `.zip` file is named according to this template:

```console
mdfmt_{version}_{runtime}_{deploymentType}.zip
```

Examples of `.zip` file names in a release:

- mdfmt_1.2.0_linux-x64_framework-dependent-net8.0.zip
- mdfmt_1.2.0_linux-x64_self-contained-net8.0.zip
- mdfmt_1.2.0_win-x64_framework-dependent-net8.0.zip
- mdfmt_1.2.0_win-x64_self-contained-net8.0.zip

To download and install Mdfmt:

- Go to the [releases](https://github.com/steinar-flatland/mdfmt/releases) page.  By default, this takes you to the latest release.  You can scroll down to see earlier releases.
- Go to the release you are interested in and download a `.zip` file for the desired runtime and deployment type.
- Unzip the `.zip` file, which creates a directory named `mdfmt`.
- Ensure that the `mdfmt` directory is on your path.

### 2.2. Checking The Version

Run the software for the first time from a shell or command prompt:

```console
mdfmt --version
```

It should print out a version number that looks something like this:

```console
1.2.3
```

This version number is managed according to the guidelines of [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html).

Of the three numbers, the leftmost is the MAJOR number, the middle is the MINOR number, and the rightmost is the PATCH number.

- MAJOR version means a breaking change.
- MINOR version means new functionality added in a backward compatible way.
- PATCH version means a backward compatible fix to pre-existing functionality.

### 2.3. Displaying Help

```console
mdfmt --help
```

Displays:

1. Semantic version number.
2. Copyright notice.
3. Licensing information.
4. Description of command line arguments and positional parameter.

If you are new to Mdfmt, it is a good idea to read the `mdfmt --help` output in consultation with this [Glossary](Glossary.md) that describes the ubiquitous language.

## 3. md File Extension

Mdfmt will only process and update Markdown files where the file name ends in `.md`

Attempting to apply Mdfmt to a file that does not end with the `.md` extension leads to the following error message:

```console
File cannot be processed because it is not a .md file
```

This behavior is a design choice to keep the application of the tool focused and to prevent erroneous editing.  Hopefully this is not inconvenient for users of the tool.  If you have a use case that requires more flexibility regarding the file extension, please [reach out](../../README.md#contact).

## 4. Usage Overview

Mdfmt is a CLI that you call from a command prompt or shell.  An Mdfmt command follows this basic template:

```console
mdfmt {options} {target-path}
```

The basic idea is that the specified `{options}` indicate formatting to be done to either a single Markdown file specified as the `{target-path}`, or to all the Markdown files contained in a directory specified as the `{target-path}`.

If an explicit target path is omitted from the commandd line, it defaults to the current working directory.

**Example:**

```console
mdfmt --heading-numbers 1. ./My-Document.md
```

The above command adds heading numbers like the ones that this [Mdfmt User's Guide](#mdfmt-users-guide) contains, to a Markdown file named `My-Document.md` in the current directory.  The formatting option `--heading-numbers 1.` means that heading numbers should be added, and each number should end with a dot.  The first section will be numbered `1.` and if it has subsections, they will be numbered `1.1.`, `1.2.`, etc.

**Example:**

```console
mdfmt --heading-numbers none --flavor Common -r /my-project/docs
```

The above command both removes heading numbers and assures that the `Common` [slugification](./Glossary.md#slug) algorithm is used on all in-document links so that they work on common platforms including GitHub and VS Code Markdown preview.  In this example, the target path is a directory (the `docs` folder of a project called `my-project`), and the `-r` option applies the specified formatting options to all Markdown files that are either directly in the `docs` folder or in a subfolder that is a descendant of `docs`.

For more information about available options, run `mdfmt --help`.  For a deeper discussion, see the next section on [Options And Target Path](#5-options-and-target-path) below.

## 5. Options And Target Path

This section discusses the command line options and target path that are passed to Mdfmt on the command line.

To best understand this section, be familiar with the [Usage Overview](#4-usage-overview) above, and what is meant by the terms _options_ and _target path_ in the context of Mdfmt usage.

A few words about how each option is described in the subsections below below:

- Long name - The long name of an option, for specifying it on the command line, e.g. `--recursive`.  All options have a long name.  
- Short name - A shortened form of the long name, e.g. `-r`.  Many, but not all options have a short name.  When an option has both a long name and a short name, they are synonyms, and which one you use is a matter of style.

  > Terminology note:  Collectively, long names and short names are referred to simply as _names_ of options.

- Type - The type of option.  The different types are:
  - flag - an option that is specified with a name only and without another token to provide a value.  A flag is used to pass a boolean value to an option of the program in the following way:  When the option's name is provided, the value passed to the option is `true`.  When the option's name is not provided, the value pased to the option is `false`.

    > flag is the only type where the name of the option is not followed by another token to provide the value.  All the other types do require a value after the name, for example `-h 1`

  - enumeration - an option that takes as input one value from a list of possible values.
  - nonnegative int - an option that takes as input an integer that is `>= 0`.  Passing a value that is out of range results in an error message.
  - string - a string of characters.
- Description - A description of the effect of the option; what it does; how it works.
- Values - Describes the valid values of the Type.  For an enumeration, lists the values and explains what each one means.
- Default - Some options have a default value, which is the value that the option assumes when it is omitted from the command line.  When there is no default, omission of the option from the command line leads to a `null`/missing value.  The Mdfmt program reacts to this either by providing an error message if the omission is unacceptable, or by implementing a default behavior if the omission is acceptable.
- Configuration file key - When using configuration files (see the section, [Configuration](#7-configuration)), provides the key to use to identify this formatting option in configuration files.  Configuration file key is only provided for [formatting options](#54-formatting-options), not for other command line options.

### 5.1. Traversal Options

These options control how the program traverses the file system when a directory is being processed.  At this time there is only one option (`--recursive, -r`).

#### 5.1.1. Recursive

- Long name: **`--recursive`**
- Short name: **`-r`**
- Type: flag
- Description: This option is applicable only when the target path is a directory, and it is ignored when the target path is a specific Markdown file.  When true, processes Markdown files both in the target directory and in all subfolders.  When false, processes Markdown files in the target directory only, not in subfolders.
- Values: true, false
- Default: false

### 5.2. Informational Options

These options allow you to:

- Control how much information Mdfmt prints out when it runs.
- Get help about the program and its available options.
- Display the software version number.

#### 5.2.1. Verbose

- Long name: **`--verbose`**
- Short name: **`-v`**
- Type: flag
- Description:  Controls how much output is written to the console when Mdfmt runs.  Mdfmt always reports warnings and errors, independent of this option.  When this option is false, Mdfmt reports the name of each file that is modified.  When true, prints out much more information, including the Mdfmt version number, options used, Markdown files inspected (even if not modified), and what types of changes (if any) were made to each file.  For further details, please see the section on [Output](#6-output).
- Values: true, false
- Default: false

#### 5.2.2. Help

- Long name: **`--help`**
- Short name: _none_
- Type: flag
- Description:  When true, prints out help information and exits.  When the `--help` option is used in conjunction with any other options, the other options are ignored.  The help comprises the following information:
  - Mdfmt version number
  - Copyright notice
  - Licensing information
  - List of available options, with a description for each
- Values: true, false
- Default: false

#### 5.2.3. Version

- Long name: **`--version`**
- Short name: _none_
- Type: flag
- Description:  When true, prints out the version number of Mdfmt and exits.  When the `--version` option is used in conjunction with any other options, the other options are ignored.
- Values: true, false
- Default: false

### 5.3. Environment

- Long name: **`--environment`**
- Short name: **`-e`**
- Type: string
- Description: Optional environment name affecting configuration.  When specified, Mdfmt requires the presence of a configuration file named `mdfmt.{environment}.json`, where `{environment}` is replaced by the value passed to this option.
- Values: Use a string with characters that are valid to use as part of file names in your operating system's file system.
- Defult: `null`.  If this option is omitted, Mdfmt does not look for an environment-specific configuration file.

For full details on the configuration system, see [Configuration](#7-configuration).

### 5.4. Formatting Options

These options are used to specify formatting that Mdfmt should apply to Markdown files targeted by the [target path](#55-target-path) that can be specified on the command line.

#### 5.4.1. Flavor

- Long name: **`--flavor`**
- Short name: **`-f`**
- Type: enumeration
- Description: Sets the [slugification](./Glossary.md#slugification) algorithm used to convert headings to link destinations.  This affects in-document links both in the body and in the table of contents of the document.  Dependency: If the value of the `--toc-threshold, -t` option `> 0`, then this `--flavor, -f` option is required, to inform the flavor of TOC link destinations.
- Values:
  - `Common` - Use a common slugification algorithm that works on multiple platforms including GitHub and VS Code Markdown preview.
  - `Azure` - Use a slugification algorithm that works in Azure DevOps Wiki.
- Default: `null`.  If this option is omitted, in-document links are not updated.
- Configuration file key: `Flavor`

  > If you have a need for another slugification algorithm, it is likely an easy addition to make.  The code has been architected with an interface that can be implemented for more algorithms.  Feel free to [reach out](../../README.md#contact).

#### 5.4.2. Heading Numbers

- Long name: **`--heading-numbers`**
- Short name: **`-h`**
- Type: enumeration
- Description: Whether to include heading numbers and, if so, the style of heading numbers to use.  When Mdfmt applies heading numbers, it looks for new sections and misnumbered headings, adjusting heading numbers throughout the document as needed.  This makes it easy to insert a new section between others:  Just insert a new heading without a heading number and run `mdfmt -h`.  Mdfmt assigns the a heading number and fixes any heading numbers after the new section.

  Heading numbers are part of the text of the heading.  Changing heading numbers breaks links within the document, since link destinations incorporate the heading numbers.  By itself, the `-h` option does not fix links broken by adjusting heading numbers.  It can't, because with just the `-h` option, there is no input about the slugification algorithm to use.  To automatically patch up the links that are broken by heading number changes, be sure to also provide the `-f` option.

  Mdfmt only pays attention to Markdown headings with up to six pound signs (`######`).  Would-be headings with seven or more `#` symbols are ignored by Mdfmt, and it does not manage heading numbers on such lines.

- Values:
  - `none` - Remove heading numbers.
  - `1.` - Include heading numbers that end in a period.  The first section is numbered `1.`, and its children are `1.1.`, `1.2.`, ... etc.  Note that this [Mdfmt User's Guide](#mdfmt-users-guide) document uses the `-h 1.` option.
  - `1` - Include heading numbers that do not end in a period.  The first section is numbered `1`, and its children are `1.1`, `1.2`, ... etc.
- Default: `null`.  If this option is omitted, no changes are made to heading numbers.
- Configuration file key: `HeadingNumbering`

#### 5.4.3. TOC Threshold

- Long name: **`--toc-threshold`**
- Short name: **`-t`**
- Type: nonnegative int
- Description: The minimum number of headings for which to include a table of contents (TOC).
  - When 0:  Always assures removal of any TOC.
  - When a positive int:  If the number of headings in the document meets or exceeds the threshold, ensures that a TOC is added or updated, and if the number of headings is below the threshold, ensures removal of the TOC.
    - Dependency:  The `-f` option is required when the value of `-t` > 0, so that TOC generation knows how to make link destinations.
- Default: `null`.  If this option is omitted, threshold-based TOC maintenance does not occur; however, a pre-existing TOC can still be maintained if the `-f` option is specified in this case.
- Configuration file key: `TocThreshold`

#### 5.4.4. Line Numbering Threshold

- Long name: **`--line-numbering-threshold`**
- Short name: **`-l`**
- Type: nonnegative int
- Description: The minimum number of lines in a fenced code block, for line numbering.
  - When 0: Always assures removal of line numbers from fenced code blocks.
  - When a positive int: If the number of lines in a fenced code block meets or exceeds the threshold, ensures that each line of the code block starts with a 1-based line number, and if the number of lines is below the threshold, ensures removal of the line numbers.
  - Default: `null`.  If this option is omitted, Mdfmt does not edit line numbers in fenced code blocks.
  - Configuration file key: `LineNumberingThreshold`

#### 5.4.5. Newline Strategy

- Long name: **`--newline-strategy`**
- Short name: _none_
- Type: enumeration
- Description:  Strategy for maintaining newlines.
- Values:
  - `Unix` - Use Unix newlines (`\n`).
  - `Windows` - Use Windows newlines (`\r\n`).
  - `PreferUnix` - Prefer Unix newlines (`\n`).
  - `PreferWindows` - Prefer Windows newlines (`\r\n`).

  The non-preferred options force all newlines to the specified type.

  The preferred options only take effect if the file being processed has a mixture of different kinds of newlines:  Then, all newlines are switched over to the preference, and any new newlines introduced by Mdfmt also follow the preference.  If, the file being processed has only one kind of newline, Mdfmt ignores the preferred option and just continues to use the kind of newline that is already being used.

- Default: `null`.  If this option is omitted, no changes are made to existing newlines, and any new newlines introduced by Mdfmt follow the predominant style of the current file.  (In the event of a tie, Mdfmt uses `\n` for newly added newlines.)
- Configuration file key: `NewlineStrategy`

### 5.5. Target Path

The target path is a so called "positional argument" passed on the command line, which means it does not need to be preceded by an option name, i.e. no long name or short name.

- Long name: _not applicable_
- Short name: _not applicable_
- Type: string
- Description:  The path of either a single Markdown file or of a directory.  If a Markdown file is specified, its name must end in `.md` (see [md File Extension](#3-md-file-extension)).

  - If a Markdown file is specified, only that file is processed.
  - If a directory is specified, the files contained in that directory are processed.  This may or may not be done recursively, depending on the [-r](#511-recursive) option.

- Default: `.` (the current working directory, i.e. the directory that is current when Mdfmt is launched)

## 6. Output

Independent of the verbosity (`-v`) setting, Mdfmt always displays errors and warnings on the console.

- When updating links, if unable to match a link to a heading, Mdfmt displays a message like `C:\path\file.md: Could not match link to heading: [label](destination)`.  This helps you notice and correct broken links within your document.  Any warnings that occur are written to the console in yellow.
- Errors can occur for a variety of reasons including:
  - Invalid options and/or path passed in on the command line.
  - Invalid JSON in .mdfmt file.
  - Unexpected exceptions thrown by the program.

  Any errors that occur are written to the console in red.

### 6.1. Non-Verbose Output

In non-verbose mode (without the `-v` or `--verbose` flag), the output written to the console by Mdfmt is minimal.  In addition to errors and warnings, Mdfmt writes the full path names of modified files as they are saved.

### 6.2. Verbose Output

Verbose output includes more information, providing a bit more insight into what Mdfmt is doing.  In addition to errors and warnings, Mdfmt writes the following information to the console:

- The Mdfmt version number.
- The [current working directory](./Glossary.md#current-working-directory).
- A `CommandLineOptions` object, showing the options and [target path](./Glossary.md#target-path) parsed from the command line.  Note that the target path may be relative or absolute, depending upon how it was specified.  Only options with non-null values are shown.
- The target path as an absolute path, for clarity.
- The names of options that were explicitly set on the command line.
- The [processing root](./Glossary.md#processing-root) directory, which both contains any configuration files and defines the scope of the Markdown files visible to Mdfmt.  See the section on [How Configuration Files Are Loaded](#72-how-configuration-files-are-located) for a discussion of how the processing root is determined and configuration files are located.
- The path name(s) and (combined) content of the loaded configuration files, if any, is shown.  See also the next section [Configuration](#7-configuration).
- For each file processed during the Mdfmt run:

  - In cyan, a line showing `Processing` and the full path of the file being processed.
  - The `FormattingOptions` being applied to the file.  Only options that are non-null are shown.
  - The number of regions and headings loaded as part of loading the Markdown file.
  - If the file is modified and saved, a summary of actions taken is shown in green.  The possible actions are:
    - Updated heading numbers
    - Updated link with label {label} to target destination {destination}
    - Inserted new TOC
    - Updated TOC
    - Removed TOC
    - Added line numbers to fenced code block(s)
    - Removed line numbers from fenced code block(s)
    - Wrote file {filePath}

Below is an example of verbose output.  Imagine that this user's guide is in a state of having no heading numbers or TOC.  Also, many of the links are incompatible with the Common flavor of Markdown.  In this state, the following mdfmt command is applied, with verbose output requested through the `-v` option:

![image](.assets/verbose-output-example-1.png)

Repeating the same command a second time leads to different verbose output, since the user's guide was already in the requested state:

![image](.assets/verbose-output-example-2.png)

Now, there are no green lines of output, reflecting the fact that Mdfmt did not need to do any formatting or write the file.  The file was already in the correct state.

Running a simpler command that requests verbose output but provides no formatting options produces the following verbose output:

![image](.assets/verbose-output-example-3.png)

Note that fewer `CommandLineOptions` are shown, since only provided (non-null) options are shown.

The end of the output shows that the `FormattingOptions` are empty, since no formatting options were provided.  With no formatting options, Mdfmt is not being asked to do any work, so of course there is no green output and the file is not modified or saved.

## 7. Configuration

Mdfmt supports configuration files for specifying formatting options that control how the Markdown (`.md`) files within a directory (called the [processing root](./Glossary.md#processing-root)) and its subdirectories will be formatted.  This is useful when developing a directory or repository that includes numerous Markdown files, and there needs to be consistent formatting across these files.

### 7.1. Configuration File Names

Mdfmt configuration files must be correctly named to be recognized by the program.  The supported names are:

- `mdfmt.json` - Use this file to provide base formatting options, which can be overridden by formatting options from an environment-specific file (see `mdfmt.{environment}.json` below).
- `.mdfmt` _(deprecated)_ - This file name is an older way of naming `mdfmt.json`.  The newer name, `mdfmt.json`, is preferred.
- `mdfmt.{environment}.json` - Use this file to provide environment-specific formatting options.  The `{environment}` part of the name matches up with the value passed to the [`--environment, -e`](#53-environment) command line option, which allows an environment-specific configuration to be selected from the command line.

### 7.2. How Configuration Files Are Located

A [target path](./Glossary.md#target-path) is passed to Mdfmt on the command line (see also the [Target Path](#55-target-path) section under [Options and Target Path](#5-options-and-target-path)), and if none is provided explicitly, it defaults to the [current working directory](./Glossary.md#current-working-directory).

From the target path, Mdfmt knows the [target directory](./Glossary.md#target-directory).  If the target path indicates a specific Markdown file, then the target directory is the directory containing this file; otherwise, if the target path indicates a directory, then the target directory and target path are one and the same.

Next a scan for configuration files happens.  Mdfmt looks for its configuration files first in the target directory, then in the parent directory of the target directory, and then in each successive ancestor directory.  The first directory that contains one or more files named one of the [Configuration File Names](#71-configuration-file-names) is designated the [processing root](./Glossary.md#processing-root) directory.  If the scan reaches the root of the file system without finding any Mdfmt configuration files, then the processing root defaults to the target directory.  Mdfmt can only see and format Markdown files that are in the processing root or one of its subdirectories; any other Markdown files are out of scope of the Mdfmt run.

Configuration files that affect the current run must be in the same directory, together, in the processing root directory.  Common scenarios include the following:

- The processing root contains only a `mdfmt.json` (or `.mdfmt` - _deprecated_) file to govern the formatting of Markdown files during the run.
- The processing root contains both `mdfmt.json` and `mdfmt.{environment}.json`, and the environment is specified on the command line using the `--environment, -e` option.  `mdfmt.json` is loaded first as a base, and then formatting options from the environment-specific file override the base options.
- It is not necessary to use `mdfmt.json` or `.mdfmt`.  Another scenario is that the processing root contains only `mdfmt.{environment}.json` file(s), and the configuration file to use is specified on the command line with the `--environment, -e` option.

Note that if the processing root contains both `mdfmt.json` and `.mdfmt`, this is an ambiguous situation, because these file names are synonyms.  Mdfmt resolves the ambiguity by displaying a warning, ignoring `.mdfmt` (which is deprecated), and using `mdfmt.json` preferentially.

If there are no configuration files available, then the processing root is the target directory, and formatting options can be passed in on the command line.

When formatting is specified both in configuration files and on the command line, formatting options from the command line override formatting options from configuration files.

### 7.3. Configuration File Format

The configuration files mentioned in the section, [Configuration File Names](#71-configuration-file-names), all use the same JSON file format.  This section describes this single format.

The simplest valid configuration file is:

```JSON
{}
```

`{}` is semantically equivalent to the following, comprising two empty dictionaries:

```JSON
{
  "Options": {},
  "CpathToOptions": {}
}
```

The `"Options"` dictionary maps names to sets of formatting options.  It is just a way of defining named collections of formatting options that can be used.  The names are not bound to anything yet.  These bindings are specified by the next dictionary.

The `"CpathToOptions"` dictionary maps [cpaths](./Glossary.md#cpath), which are paths relative to the [processing root](./Glossary.md#processing-root), to keys in `"Options"`, thus establishing how to format the Markdown content of each cpath.  A cpath can indicate a specific Markdown file, or a directory.  If a directory, it means the formatting applies to all Markdown files in that directory recursively.

Here is an example of the `mdfmt.json` file used to format Mdfmt's own documentation:

```JSON
{
  "Options": {
    "default": {
      "Flavor": "Common",
      "TocThreshold": 3,
      "HeadingNumbering": "1.",
      "LineNumberingThreshold": 0
    },
    "noHeadings": {
      "HeadingNumbering": "none"
    }
  },
  "CpathToOptions": {
    ".": "default",
    "./user/Glossary.md": "noHeadings",
    "./developer/Releasing.md": "noHeadings"
  }
}
```

Some observations:

- Each key of the `"Options"` dictionary (such as `"default"`, `"noHeadings"`) is a descriptive name mapping to a value that is a JSON object containing name/value pairs for formatting options.
- Within the JSON object comprising a value in `"Options"`, the keys such as `Flavor`, `TocThreshold`, `HeadingNumbering`, and `LineNumbereingThreshold` are documented as the "Configuration file key:" associated with each formatting option described in the [Formatting Options](#54-formatting-options) section of this document.
- The keys of the `"CpathToOptions"` dictionary are relative paths, relative to the [processing root](./Glossary.md#processing-root).  Some rules for these keys:
  - Each key is a path, relative to the processing root, starting with `.`
  - `.` means the processing root directory.
  - If the relative path contains slashes, use forward slashes.
- The values in the `"CpathToOptions"` dictionary are keys of the `"Options"` dictionary, establishing path to options bindings.
- When a given Markdown file is processed by Mdfmt, multiple `"CpathToOptions"` bindings may apply to it.  Any binding where the `"CpathToOptions"` key is a prefix of the file's cpath, apply.  More specific options, i.e. those that were selected based on a longer `"CpathToOptions"` key override more general options.  For more details, please refer to [Options Inheritance](#75-options-inheritance).

### 7.4. Loading Multiple Configuration Files

As discussed in in the section on [Configuration File Names](#71-configuration-file-names), it is possible to provide a base set of formatting options in a file, `mdfmt.json`, and then environment-specific overrides in a file, `mdfmt.{environment}.json`.  Here is a motivating example of where this could be useful:

> Documentation in a documentation repository is published to the [Backstage](https://backstage.io/docs/overview/what-is-backstage/) developer portal.  When Backstage renders Markdown for display, it automatically includes line numbers for fenced code blocks, so it would be silly/redundant to include line numbers in the content itself.  Developers edit the documentation on the local laptop using Visual Studio Code, and in this environment, there are no automatically generated line numbers.  Developers would like to be able to see line numbers locally, so they can know how to refer to lines by line number when they are writing descriptions in the documentation being maintained.  We would like to set things up so that running `mdfmt -r` formats for Backstage, and `mdfmt -r -e local` formats for local development with Mdfmt-generated line numbers in fenced code blocks.

To solve for this scenario, create a base `mdfmt.json` file that specifies how to format for Backstage.  It might look like this:

```JSON
{
  "Options": {
    "default": {
      "Flavor": "Common",
      "TocThreshold": 3,
      "HeadingNumbering": "1.",
      "LineNumberingThreshold": 0,
      "NewlineStrategy": "PreferUnix"
    }
  },
  "CpathToOptions": {
    ".": "default"
  }
}
```

The key thing is `"LineNumberingThreshold": 0`, which ensures no automatically generated line numbers in fenced code blocks.

To use the same configuration on the local laptop, but **with** Mdfmt-generated line numbers in fenced code blocks, create an environment-specific configuration file, `mdfmt.local.json`, as follows:

```JSON
{
  "Options": {
    "default": {
      "LineNumberingThreshold": 1
    }
  }
}
```

Place `mdfmt.local.json` into the same directory as `mdfmt.json`. When making the environment-specific file, it is not necessary to repeat all formatting options, nor is it necessary to repeat all the `"CpathToOptions"` bindings.  Just configure the things that should be different in the environment.  You may also add new configuration options and bindings in the environment-specific file.

In summary, when both `mdfmt.json` and `mdfmt.{environemnt}.json` are provided, `mdfmt.json` is loaded first, and then the environment-specific file is loaded on top of that, replacing parts of the base configuration and extending the base configuration as per the settings in `mdfmt.{environment}.json`.

### 7.5. Options Inheritance

After configuration files are loaded into Mdfmt, possibly combining multiple configurations as discussed in the previous section, Mdfmt implements an inheritance scheme to decide which options to apply to a given Markdown file being processed.  It works like this:

If the [cpath](./Glossary.md#cpath) of a file being processed is `./a/b/c/file.md`, Mdfmt looks for `CpathToOptions` bindings keyed on the following, in order:

1. `.`
2. `./a`
3. `./a/b`
4. `./a/b/c`
5. `./a/b/c/file.md`

The first options found form a configuration base, and more specific options, if found, will override more general options found earlier.

Finally, any formatting options passed in on the command line are considered the most specific, providing a final level of override to determine the final configuration.

In a large recursive (`-r`) Mdfmt run, this approach allows different Markdown files to be processed with different options.

#### 7.5.1. Example: Deciding How To Format The Glossary

Consider the following configuration:

```JSON
{
  "Options": {
    "default": {
      "Flavor": "Common",
      "TocThreshold": 3,
      "HeadingNumbering": "1.",
      "LineNumberingThreshold": 0
    },
    "noHeadings": {
      "HeadingNumbering": "none"
    }
  },
  "CpathToOptions": {
    ".": "default",
    "./user/Glossary.md": "noHeadings",
    "./developer/Releasing.md": "noHeadings"
  }
}
```

For example, Mdfmt needs to decide what options to use for formatting `./user/Glossary.md`.

It performs lookups on the following `CpathToOptions` keys, in order:  

- `.`
- `./user`
- `./user/Glossary.md`

The lookup of `.` leads to the `default` options, defining the initial candidate set of options to use:

```JSON
{
  "Flavor": "Common",
  "TocThreshold": 3,
  "HeadingNumbering": "1.",
  "LineNumberingThreshold": 0
}
```

Next, the lookup of `./user` does not match anything.

Next, the lookup of `./user/Glossary.md` returns `noHeadings`, which points to the following options:

```JSON
{
  "HeadingNumbering": "none"
}
```

These options are written over the candidate set found earlier, resulting in:

```JSON
{
  "Flavor": "Common",
  "TocThreshold": 3,
  "HeadingNumbering": "none",
  "LineNumberingThreshold": 0
}
```

These are the formatting options that will be applied to the glossary.  Notice the utility and flexibility of this.  It allows creation of a base configuration that applies to the common case (usually heading numbers are desirable) but in certain exceptional cases, e.g. a long flat document like a glossary where heading numbers don't add much value, a different formatting style can be applied.

## 8. Return Value

Mdfmt returns values to the shell that indicate how the program exited.  This can be useful when building scripts.

- 0 - The program exited successfully.
- 1 - The program ended with an exception.
- 2 - The program exited because of problems with the command line arguments that were provided.
