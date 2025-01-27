# Glossary

<!--BEGIN_TOC-->
- [Glossary](#glossary)
  - [cpath](#cpath)
  - [current working directory](#current-working-directory)
  - [destination](#destination)
  - [label](#label)
  - [link](#link)
  - [Markdown](#markdown)
  - [Mdfmt](#mdfmt)
  - [mdfmt configuration file](#mdfmt-configuration-file)
  - [processing root](#processing-root)
  - [slug](#slug)
  - [slugification](#slugification)
  - [table of contents](#table-of-contents)
  - [target path](#target-path)
  - [target directory](#target-directory)
  - [TOC](#toc)
  - [TOC threshold](#toc-threshold)
<!--END_TOC-->

This file defines terms that form the ubiquitous language of Mdfmt.

## cpath

the canonical relative path of a file being processed by [Mdfmt](#mdfmt).  Such a path is relative to the [processing root](#processing-root), where the [mdfmt configuration file](#mdfmt-configuration-file) (if any) is.  _Cpaths_ are used as keys in the [mdfmt configuration file](#mdfmt-configuration-file) to bind sets of formatting options to directories or to individual files.

## current working directory

the directory in which [Mdfmt](#mdfmt) was launched and that is dynamically associated with the running program.

Uses of the _current working directory_ by [Mdfmt](#mdfmt):

1. When the [target path](#target-path) that [Mdfmt](#mdfmt) is being asked to process is a relative path, this path is understood relative to the _current working directory_.
2. When determing the [processing root](#processing-root) of the [Mdfmt](#mdfmt) run, in the absence of an [mdfmt configuration file](#mdfmt-configuration-file), the _current working directory_ is used as the [processing root](#processing-root).

## destination

a URL or relative path indicating the navigation target of a [link](#link) in a [Markdown](#markdown) document.

## label

the text of a [link](#link) that is rendered as part of a [Markdown](#markdown) document as a clickable hyperlink.

## link

a construct in a [Markdown](#markdown) document that provides a navigable hyperlink.  The [Markdown](#markdown) formatting syntax for a _link_ is of the form

```Markdown
[label](destination)
```

where the [`label`](#label) is text that is rendered as part of the [Markdown](#markdown) document as a clickable hyperlink, and the [`destination`](#destination) indicates a navigation target in the form of a URL or a relative path.  The relative path may be to a [Markdown](#markdown) document, optionally with an indication of the specific heading within the target document to navigate to.  

## Markdown

created by [John Gruber](https://daringfireball.net/contact/) in 2004, he describes _Markdown_ as a "text-to-HTML conversion tool for web writers" in this [introductory article about Markdown](https://daringfireball.net/projects/markdown/).  It comprises both a plain text syntax for adding formatting to an author's content, and a tool for converting this formatted plain text to HTML so it can be rendered.  Gruber describes the most important design goal for Markdown's formatting syntax in the above mentioned article:

> The overriding design goal for Markdown’s formatting syntax is to make it as readable as possible. The idea is that a Markdown-formatted document should be publishable as-is, as plain text, without looking like it’s been marked up with tags or formatting instructions.

Since its introduction, Markdown has been widely adopted.  There are numerous implementations with varying features and details.  The [Wikipedia article on Markdown](https://en.wikipedia.org/wiki/Markdown) provides more context, history, examples, and lists many of the implementations.

## Mdfmt

a CLI tool for formatting either individual [Markdown](#markdown) files or all the [Markdown](#markdown) files found in a directory.

## mdfmt configuration file

an optional configuration file that specifies formatting options applied by [Mdfmt](#mdfmt) to [Markdown](#markdown) files.

How the file itself (not considering path) is expected to be named: If the `-e, --environment` option has been specified, the _mdfmt configuration file_ is expected to be named `mdfmt.{environment}.json`, where `{environment}` is replaced by value passed to the `-e, --environment` option.  If the `-e, --environment` is not being used, the expected name of the file is simply `.mdfmt`.

How the file is located at runtime:  [Mdfmt](#mdfmt) first looks in the [target directory](#target-directory), then in the parent directory (if any) of the [target directory](#target-directory), and on up to all ancestor directories:  The first directory that contains a file with the expected name of the _mdfmt configuration file_ defines the [processing root](#processing-root).  The configuration file is then loaded, and it governs the formatting that is applied to all [Markdown](#markdown) files that are formatted within the [processing root](#processing-root) and any of its subdirectories.  Canonical relative paths, or [cpaths](#cpath), configured in the _mdfmt configuration file_ are relative to the [processing root](#processing-root).

Formatting options explicitly provided on the command line are more powerful than, and will override, formatting options provided through the _mdfmt configuration file_.

The _mdfmt configuration file_ is optional.  If not present, then the [current working directory](#current-working-directory) is used as the [processing root](#processing-root), and only command line options govern the formatting that happens.

## processing root

a directory that defines the root of files that [Mdfmt](#mdfmt) can see.  [Mdfmt](#mdfmt) can only format `.md` files that are in the _processing root_ directory or one of its subdirectories, and the program is unaware of any other files outside the _processing root_.

If there exists an [mdfmt configuration file](#mdfmt-configuration-file), the directory containing it, by definition, is the _processing root_.

## slug

in the context of [Mdfmt](#mdfmt), a string that can be used as a [destination](#destination) of a [link](#link), to navigate to a [Markdown](#markdown) document, often to a specific section of that document.  The _slug_ is based on the target file name and heading text, transformed by a function that does things like remove non-URL-safe characters, replace space by `-`, standardize casing, etc.  The specific [slugification](#slugification) rules used vary by the target [Markdown](#markdown) rendering platform.

## slugification

in the context of [Mdfmt](#mdfmt), the process of converting a [Markdown](#markdown) file name and the text of a heading the way it displays to the user, to a [destination](#destination) that can be used in a [link](#link).  There are different slugification algorithms that support different [Markdown](#markdown) rendering environments.  For example, [Markdown rendered on GitHub](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax) requires a different format of [slug](#slug) than [Azure DevOps Wiki](https://learn.microsoft.com/en-us/azure/devops/project/wiki).

## table of contents

a region of a [Markdown](#markdown) document that [Mdfmt](#mdfmt) maintains.  It comprises a hierarchical outline of [links](#link), where each [destination](#destination) goes to a heading of the document.

## target path

a path that [Mdfmt](#mdfmt) is being asked to process, specified on the command line as an absolute or relative path indicating either an individual [Markdown](#markdown) (`.md`) file or a directory.  When a relative path, it is relative to the [current working directory](#current-working-directory).  If a directory as opposed to a specific file, it means to process the [Markdown](#markdown) files in the directory (and optionally subdirectories depending on the `-r, --recursive` option).  If the _target path_ is not specified, it defaults to the [current working directory](#current-working-directory).

## target directory

The directory of the [target path](#target-path).  There are two cases:

- If the [target path](#target-path) indicates a specific [Markdown](#markdown) (`.md`) file, then the _target directory_ is the directory containing this specific file.
- If the [target path](#target-path) indicates a directory, then the _target directory_ and [target path](#target-path) are one and the same.

## TOC

short for [table of contents](#table-of-contents).

## TOC threshold

an integer greater than or equal to 0, indicating the minimum number of headings that a document must have before a [TOC](#toc) is generated.  A _TOC threshold_ of 0 causes an existing table of contents to be removed.
