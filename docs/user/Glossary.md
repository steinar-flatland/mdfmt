# Glossary

<!--BEGIN_TOC-->
- [Glossary](#glossary)
  - [cpath](#cpath)
  - [current working directory](#current-working-directory)
  - [destination](#destination)
  - [label](#label)
  - [link](#link)
  - [Markdown](#markdown)
  - [.mdfmt file](#mdfmt-file)
  - [Mdfmt](#mdfmt)
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

the canonical relative path of a file being processed by [Mdfmt](#mdfmt).  Such a path is relative to the [processing root](#processing-root), where the [.mdfmt file](#mdfmt-file) (if any) is.  _Cpaths_ are used as keys in the [.mdfmt file](#mdfmt-file) to bind sets of formatting options to directories or to individual files.

## current working directory

the directory in which [Mdfmt](#mdfmt) was launched and that is dynamically associated with the running program.

Uses of the _current working directory_ by [Mdfmt](#mdfmt):

1. When the [target](target) that [Mdfmt](#mdfmt) is being asked to process is a relative path, this path is understood relative to the _current working directory_.
2. When determing the [processing root](#processing-root) of the [Mdfmt](#mdfmt) run, in the absence of an [.mdfmt file](#mdfmt-file), the _current working directory_ is used as the [processing root](#processing-root).

## destination

a URL or relative path indicating the navigation target of a [link](#link) in a [Markdown](#markdown) document.

## label

the text of a [link](#link) that is rendered as part of a [Markdown](#markdown) document as a clickable hyperlink.

## link

a construct in a [Markdown](#markdown) document that provides a navigable hyperlink.  The [Markdown](#markdown) formatting syntax for a _link_ is of the form

```Markdown
[label](destination)
```

where the [`label`](#label) is text that is rendered as part of the [Markdown](#markdown) document as a clickable hyperlink, and the [`destination`](#destination) indicates a navigation target in the form of a URL or a relative path to a [Markdown](#markdown) document, optionally with an indication of the specific heading within the target document to navigate to.  

## Markdown

created by [John Gruber](https://daringfireball.net/contact/) in 2004, he describes _Markdown_ as a "text-to-HTML conversion tool for web writers" in this [introductory article about Markdown](https://daringfireball.net/projects/markdown/).  It comprises both a plain text syntax for adding formatting to an author's content, and a tool for converting this formatted plain text to HTML so it can be rendered.  Gruber describes the most important design goal for Markdown's formatting syntax in the above mentioned article:

> The overriding design goal for Markdown’s formatting syntax is to make it as readable as possible. The idea is that a Markdown-formatted document should be publishable as-is, as plain text, without looking like it’s been marked up with tags or formatting instructions.

Since its introduction, Markdown has been widely adopted.  There are numerous implementations with varying features and details.  The [Wikipedia article on Markdown](https://en.wikipedia.org/wiki/Markdown) provides more context, history, examples, and lists many of the implementations.

## .mdfmt file

a configuration file that specifies formatting options applied by [Mdfmt](#mdfmt) to [Markdown](#markdown) files in the directory where the file named `.mdfmt` exists and in subdirectories thereof.  The location of this file also defines the [processing root](#processing-root).  Note that explicit command line options override settings from the _.mdfmt file_.

## Mdfmt

a CLI tool for formatting either individual [Markdown](#markdown) files or all the [Markdown](#markdown) files found in a directory.

## processing root

a directory that defines the root of files that [Mdfmt](#mdfmt) can see.  [Mdfmt](#mdfmt) can only format `.md` files that are in the _processing root_ directory or one of its subdirectories, and the program is unaware of any other files outside the _processing root_.

[Mdfmt](#mdfmt) determines the processing root on startup as follows:

- Starting in the [target directory](target-directory), go to the parent directory and to each successive ancestor directory.  The first directory that contains a [.mdfmt file](#mdfmt-file) is the _processing root_.
- If there exists no [.mdfmt file](#mdfmt-file) in the [target directory](target-directory) or in any of its ancestor directories, then use the [current working directory](#current-working-directory) as the _processing root_.

Canonical relative paths, or [cpaths](#cpath), configured in the optional [.mdfmt file](#mdfmt-file), are relative to the _processing root_ directory.

## slug

in the context of [Mdfmt](#mdfmt), a string that can be used as a [destination](#destination) of a [link](#link), to navigate to a [Markdown](#markdown) document, often to a specific section of that document.  The _slug_ is based on the target file name and heading text, transformed by a function that does things like remove non-URL-safe characters, replace space by `-`, standardize casing, etc.  The specific [slugification](#slugification) rules used vary by the target [Markdown](#markdown) rendering platform.

## slugification

in the context of [Mdfmt](#mdfmt), the process of converting a [Markdown](#markdown) file name and the text of a heading the way it displays to the user, to a [destination](#destination) that can be used in a [link](#link).  There are different slugification algorithms that support different [Markdown](#markdown) rendering environments.  For example, [Markdown rendered on GitHub](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax) requires a different format of slug than [Azure DevOps Wiki](https://learn.microsoft.com/en-us/azure/devops/project/wiki).

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

an integer greater than or equal to 0, indicating the minimum number of headings that a document should have before a [TOC](#toc) is generated.  A _TOC threshold_ of 0 causes an existing table of contents to be removed.
