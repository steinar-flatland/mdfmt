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
  - [5. Options And Path](#5-options-and-path)
    - [5.1. Traversal Options](#51-traversal-options)
      - [5.1.1. Recursive](#511-recursive)
    - [5.2. Informational Options](#52-informational-options)
      - [5.2.1. Verbose](#521-verbose)
      - [5.2.2. Help](#522-help)
      - [5.2.3. Version](#523-version)
    - [5.3. Formatting Options](#53-formatting-options)
      - [5.3.1. Flavor](#531-flavor)
      - [5.3.2. Heading Numbers](#532-heading-numbers)
      - [5.3.3. TOC Threshold](#533-toc-threshold)
      - [5.3.4. Newline Strategy](#534-newline-strategy)
    - [5.4. Path](#54-path)
  - [6. Output](#6-output)
    - [6.1. Non-Verbose Output](#61-non-verbose-output)
    - [6.2. Verbose Output](#62-verbose-output)
  - [7. Using a .mdfmt File](#7-using-a-mdfmt-file)
    - [7.1. Overview And Motivation](#71-overview-and-motivation)
    - [7.2. Example of .mdfmt File](#72-example-of-mdfmt-file)
    - [7.3. How .mdfmt Options Are Evaluated](#73-how-mdfmt-options-are-evaluated)
    - [7.4. What Triggers The Use Of .mdfmt](#74-what-triggers-the-use-of-mdfmt)
  - [8. Return Value](#8-return-value)
<!--END_TOC-->

## 1. Introduction

Mdfmt is a command line interface (CLI) for Markdown formatting.  Able to operate on individual Markdown files and on directory structures containing many Markdown files, it offers assistance with automated maintenance of heading numbers, in-document links, table of contents, and consistent newlines.

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

- mdfmt_0.3.8_linux-x64_framework-dependent-net8.0.zip
- mdfmt_0.3.8_linux-x64_self-contained-net8.0.zip
- mdfmt_0.3.8_win-x64_framework-dependent-net8.0.zip
- mdfmt_0.3.8_win-x64_self-contained-net8.0.zip

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
mdfmt {options} {path}
```

The basic idea is that the specified `{options}` indicate formatting to be done to either a single Markdown file specified as the `{path}` or to all the Markdown files contained in a directory specified as the `{path}`.

In a technical sense, an option is a variable or property in the Mdfmt program, to which a value is being passed from the command line used to invoke the CLI.

**Example:**

```console
mdfmt --heading-numbers 1. ./My-Document.md
```

The above command adds heading numbers like the ones that this [Mdfmt User's Guide](#mdfmt-users-guide) contains, to a Markdown file named `My-Document.md` in the current directory.  The formatting option `--heading-numbers 1.` means that heading numbers should be added, and each number should end with a dot.  The first section will be numbered `1.` and if it has subsections, they will be numbered `1.1.`, `1.2.`, etc.

**Example:**

```console
mdfmt --heading-numbers none --flavor Common -r /my-project/docs
```

The above command both removes heading numbers and assures that the `Common` [slugification](./Glossary.md#slug) algorithm is used on all in-document links.  This makes the links work on common platforms including GitHub and VS Code Markdown preview.  In this example, the path is a directory (the `docs` folder of a project called `my-project`), and the `-r` option applies the specified formatting options to all Markdown files that are either directly in the `docs` folder or in a subfolder that is a descendant of `docs`.

For more information about available options, run `mdfmt --help`.  For a deeper discussion, see the section on [Options And Path](#5-options-and-path) below.

## 5. Options And Path

This section discusses the command line options and path that are passed to Mdfmt on the command line.

To best understand this section, be familiar with the [Usage Overview](#4-usage-overview) above, and what is meant by the terms _options_ and _path_ in the context of Mdfmt usage.

A few words about how each option is described in the subsections below below:

- Long name - The long name of an option, for specifying it on the command line, e.g. `--recursive`.  All options have a long name.  
- Short name - A shortened form of the long name, e.g. `-r`.  Many, but not all options have a short name.  When an option has both a long name and a short name, they are synonyms, and which one you use is a matter of style.

  > Terminology note:  Collectively, long names and short names are referred to simply as _names_ of options.

- Type - The type of option.  The different types are:
  - flag - an option that is specified with a name only and without another token to provide a value.  A flag is used to pass a boolean value to an option of the program in the following way:  When the option's name is provided, the value passed to the option is `true`.  When the option's name is not provided, the value pased to the option is `false`.

    > flag is the only type where the name of the option is not followed by another token to provide the value.  All the other types do require a value after the name, for example `-h 1`

  - enumeration - an option that takes as input one value from a list of possible values.
  - nonnegative int - an option that takes as input an integer that is `>= 0`.  Passing a value that is out of range results in an error message.
- Description - A description of the effect of the option; what it does; how it works.
- Values - Describes the valid values of the Type.  For an enumeration, lists the values and explains what each one means.
- Default - Some options have a default value, which is the value that the option assumes when it is omitted from the command line.  When there is no default, omission of the option from the command line leads to a `null`/missing value.  The Mdfmt program reacts to this either by providing an error message if the omission is unacceptable, or by implementing a default behavior if the omission is acceptable.

In addition, some details about the path are covered in the last subsection of this section.

### 5.1. Traversal Options

These options control how the program traverses the file system when a directory is being processed.  At this time there is only one option (`-r`, `--recursive`).

> An idea for future work is to augment traversal options with filtering options, to limit the files that are processed during a recursive traversal.

#### 5.1.1. Recursive

- Long name: **`--recursive`**
- Short name: **`-r`**
- Type: flag
- Description: This option is applicable only when the path is a directory, and it is ignored when the path is a specific Markdown file.  When true, processes Markdown files both in the path directory and in all subfolders.  When false, processes Markdown files in the path directory only, not in subfolders.
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

### 5.3. Formatting Options

These options are used to specify formatting that Mdfmt should apply to Markdown files targeted by the path that is passed on the command line.

#### 5.3.1. Flavor

- Long name: **`--flavor`**
- Short name: **`-f`**
- Type: enumeration
- Description: Sets the [slugification](./Glossary.md#slugification) algorithm used to convert headings to link destinations.  This affects in-document links both in the body and in the table of contents of the document.
- Values:
  - `Common` - Use a common slugification algorithm that works on multiple platforms including GitHub and VS Code Markdown preview.
  - `Azure` - Use a slugification algorithm that works in Azure DevOps Wiki.

  > If you have a need for another slugification algorithm, it is likely an easy addition to make.  The code has been architected with an interface that can be implemented for more algorithms.  Feel free to [reach out](../../README.md#contact).

- Default: `null`.  If this option is omitted, in-document links are not updated.

#### 5.3.2. Heading Numbers

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

#### 5.3.3. TOC Threshold

- Long name: **`--toc-threshold`**
- Short name: **`-t`**
- Type: nonnegative int
- Description: The minimum number of headings for which to include a table of contents (TOC).
  - When 0:  Always assures removal of any TOC.
  - When a positive int:  If the number of headings in the document meets or exceeds the threshold, ensures that a TOC is added or updated, and if the number of headings is below the threshold, ensures removal of the TOC.
    - Dependency:  The `-f` option is required when the value of `-t` > 0, so that TOC generation knows how to make link destinations.
- Default: `null`.  If this option is omitted, threshold-based TOC maintenance does not occur; however, a pre-existing TOC can still be maintained if the `-f` option is specified in this case.

#### 5.3.4. Newline Strategy

- Long name: **`--newline-strategy`**
- Short name: _none_
- Type: enumeration
- Description:  Strategy for maintaining newlines.
- Values:
  - `Unix` - Use Unix newlines (`\n`).
  - `Windows` - Use Windows newlines (`"\r\n"`).
  - `PreferUnix` - Prefer Unix newlines (`\n`).
  - `PreferWindows` - Prefer Windows newlines (`"\r\n"`).

  The non-preferred options force all newlines to the specified type.

  The preferred options only take effect if the file being processed has a mixture of different kinds of newlines:  Then, all newlines are switched over to the preference, and any new newlines introduced by Mdfmt also follow the preference.  If, the file being processed has only one kind of newline, Mdfmt ignores the preferred option and just continues to use the kind of newline that is already being used.

- Default: `null`.  If this option is omitted, no changes are made to existing newlines, and any new newlines introduced by Mdfmt follow the predominant style of the current file.  (In the event of a tie, Mdfmt uses `\n` for newly added newlines.)

### 5.4. Path

- Type: string
- Description:  The path of either a single Markdown file or of a directory.  If a Markdown file is specified, its name must end in `.md` (see [md File Extension](#3-md-file-extension)).

  - If a Markdown file is specified, only that file is processed.
  - If a directory is specified, the files contained in that directory are processed.  This may or may not be done recursively, depending on the [-r](#511-recursive) option.

- Default: `.` (the current directory)

## 6. Output

Independent of the verbosity (`-v`) setting, Mdfmt always displays errors and warnings on the console.

- When updating links, if unable to match a link to a heading, Mdfmt displays a message like `.\path\file.md: Could not match link to heading: [label](destination)`.  This helps you notice and correct broken links within your document.  Any warnings that occur are written to the console in yellow.
- Errors can occur for a variety of reasons including:
  - Invalid options and/or path passed in on the command line.
  - Invalid JSON in .mdfmt file.
  - Unexpected exceptions thrown by the program.

  Any errors that occur are written to the console in red.

### 6.1. Non-Verbose Output

In non-verbose mode (without the `-v` or `--verbose` flag), the output written to the console by Mdfmt is minimal.  In addition to errors and warnings, Mdfmt writes the names of modified files as they are saved.

### 6.2. Verbose Output

Verbose output includes more information, providing a bit more insight into what Mdfmt is doing.  In addition to errors and warnings, Mdfmt writes the following information to the console:

- The version of Mdfmt used.
- A `CommandLineOptions` object, showing the options and path parsed from the command line.  Only options with non-null values are shown.
- The names of options that were explicitly set on the command line are shown.
- The loaded `.mdfmt` file, if any, is shown.  See the next section [Using a .mdfmt File](#7-using-a-mdfmt-file) for more information about the `.mdfmt` files.
- For each file processed during the Mdfmt run:

  - In cyan, a line showing `Processing` and the file being processed.
  - The `FormattingOptions` being applied to the file.  Only options that are non-null are shown.
  - The number of regions and headings loaded as part of loading the Markdown file.
  - If the file is modified and saved, a summary of actions taken is shown in green.  The possible actions are:
    - Updated heading numbers
    - Updated link with label {label} to target destination {destination}
    - Inserted new TOC
    - Updated TOC
    - Removed TOC
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

## 7. Using a .mdfmt File

### 7.1. Overview And Motivation

A `.mdfmt` file allows you to configure formatting options that govern how the `.md` files within a directory and its subdirectories are formatted.  This can be useful when developing a documentation repository, and there needs to be consistent formatting across numerous Markdown files organized under a root directory.

A `.mdfmt` file allows configuration of the following:

- Named collections of formatting options.
- Bindings between relative paths ([cpaths](./Glossary.md#cpath)) of directories and/or Markdown files to these named collections of formatting options.

This provides a flexible mechanism which, as explained below, implements an inheritance scheme for options.  It is important to have flexibility.  When working on a documentation repository, is might be common for many documents to be formatted a certain way, but there can be exceptions.  For example, heading numbering might typically be desired, but in a glossary, which has a long flat outline organized by term, section numbers would be distracting.  It is useful to be able to define base formatting options that can be overridden in specific directories or files.  The `.mdfmt` file provides this.

### 7.2. Example of .mdfmt File

As an example, here is the [.mdfmt file](https://github.com/steinar-flatland/mdfmt/blob/main/docs/.mdfmt) from the [docs directory](https://github.com/steinar-flatland/mdfmt/tree/main/docs) of the [mdfmt respository](https://github.com/steinar-flatland/mdfmt):

```JSON
{
  "Options": {
    "default": {
      "Flavor": "Common",
      "TocThreshold": 3,
      "HeadingNumbering": "1."
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

There are two named sets of options:

1. There is one called `default` that defines a Markdown flavor, a TOC threshold, and a heading numbering style.
2. There is another called `noHeadings`, that calls for no heading numbers.

The `CpathToOptions` section contains bindings **from** paths that are relative to the `mdfmt/docs` directory (where the `.mdfmt` file resides) **to** option set names.  In the keys in the `CpathToOptions` section, `.` represents `mdfmt/docs`, the directory containing the `.mdfmt` file.

The [cpath](./Glossary.md#cpath) `.` matches each `.md` file under `mdfmt/docs`, no matter how deeply the file is nested in subdirectories.  The `default` options apply to all these `.md` files.  Think of this as a formatting base class.  Some or all of these options can be overridden with more specific `CpathToOptions` bindings.

The [cpath](./Glossary.md#cpath) `./user/Glossary.md` is bound to `noHeadings`.  `./user/Glossary.md` inherits the `default` options, and then the more specific `noHeadings` option set is applied, changing the inherited value of `1.` for `HeadingNumbering` to `none`.

### 7.3. How .mdfmt Options Are Evaluated

Inheritance of options works over multiple levels.  If the cpath of a file being processed is `./a/b/c/file.md`, Mdfmt looks for `CpathToOptions` bindings keyed on the following, in order:

1. `.`
2. `./a`
3. `./a/b`
4. `./a/b/c`
5. `./a/b/c/file.md`

The first options found become the base options, and more specific options, if found, will override more general options found earlier.

Finally, any formatting options passed in on the command line are considered the most specific, providing a final level of override to determine the final configuration.

In this way, a formatting options configuration is determined for each file processed during a recursive (`-r`) Mdfmt run, allowing for different Markdown files to be processed with different options.

### 7.4. What Triggers The Use Of .mdfmt

If the path passed to Mdfmt on the command line is a directory, and this directory contains a file named `.mdfmt`, this triggers the loading and use of the `.mdfmt` file.

This is a bit limiting, and needs to be improved.  It means that the only way to apply a `.mdfmt` file when working with a large repository of documentation is recursively from the root.  

> Idea for future improvement: If there is no `.mdfmt` file in the directory being processed, scan the parent directories for `.mdfmt` files and use the nearest one that is found.

## 8. Return Value

Mdfmt returns values to the shell that indicate how the program exited.  This can be useful when building scripts.

- 0 - The program exited successfully.
- 1 - The program ended with an exception.
- 2 - The program exited because of problems with the command line arguments that were provided.
