# Glossary

<!--BEGIN_TOC-->
- [Glossary](#glossary)
  - [cpath](#cpath)
  - [cross-document link](#cross-document-link)
  - [current working directory](#current-working-directory)
  - [destination](#destination)
  - [environment](#environment)
  - [external link](#external-link)
  - [fenced code block](#fenced-code-block)
  - [in-document link](#in-document-link)
  - [internal link](#internal-link)
  - [label](#label)
  - [line numbering threshold](#line-numbering-threshold)
  - [link](#link)
  - [Markdown](#markdown)
  - [Mdfmt](#mdfmt)
  - [mdfmt configuration files](#mdfmt-configuration-files)
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

the canonical relative path of a file being processed by [Mdfmt](#mdfmt).  Such a path is relative to the [processing root](#processing-root), where any [mdfmt configuration files](#mdfmt-configuration-files) are.  _Cpaths_ are used as keys in the [mdfmt configuration files](#mdfmt-configuration-files) to bind sets of formatting options to directories or to individual files.

## cross-document link

a [link](#link) targeting another [Markdown](#markdown) document, optionally with an indication of the specific heading to navigate to.

## current working directory

the directory in which [Mdfmt](#mdfmt) was launched and that is dynamically associated with the running program.  The _current working directory_ is important for resolving omitted and relative [target path](#target-path).

## destination

a URL or relative path indicating the navigation target of a [link](#link) in a [Markdown](#markdown) document.

## environment

when developting [Markdown](#markdown) files, _environment_ refers to where the files will be rendered.  Examples include a website such as [Github](https://github.com), a developer laptop, or a developer portal such as [Backstage](https://backstage.io).  [Mdfmt](#mdfmt) supports configuring different formatting options per _environment_, and designating the target _environment_ for formatting on the command line.

## external link

a [link](#link) targeting `http` or `https`.

## fenced code block

a region of [Markdown](#markdown) that both starts with and ends with a line where the first non-whitespace characters are three backquotes.  Markdown rendering shows such a region as a block of code. [Mdfmt](#mdfmt) supports adding line numbers to such code blocks.  See [line numbering threshold](#line-numbering-threshold).

## in-document link

a [link](#link) targeting a specific heading within the same [Markdown](#markdown) document.

## internal link

a collective term for a [link](#link) that is either an [in-document link](#in-document-link) or a [cross-document link](#cross-document-link).

## label

the text of a [link](#link) that is rendered as part of a [Markdown](#markdown) document as a clickable hyperlink.

## line numbering threshold

an integer greater than or equal to 0, indicating the minimum number of lines in a [fenced code block](#fenced-code-block) for which [Mdfmt](#mdfmt) will ensure line numbers.  A _line numbering threshold_ of 0 removes line numbering from [fenced code blocks](#fenced-code-block).

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

## mdfmt configuration files

one or more optional configuration files immediately in the [processing root](#processing-root) directory (not in a subdirectory), specifying formatting options applied by [Mdfmt](#mdfmt) to [Markdown](#markdown) files.

_Mdfmt configuration files_ are recognizable by their names:

- `.mdfmt` - This file plays the same role as `mdfmt.json` (see next bullet).  The file name, `.mdfmt`, is deprecated; use the name `mdfmt.json` instead.
- `mdfmt.json` - Base formatting options that apply to all Markdown files in the folder where specified and in all subfolders.  These base options apply unless overridden in an [environment](#environment)-specific _configuration file_ (see next bullet).
- `mdfmt.{environment}.json` - File with [environment](#environment)-specific formatting overrides.  This file must be in the same directory as `mdfmt.json`.  It is used to override and extend settings from `mdfmt.json`, for a specific [environment](#environment).

Formatting options explicitly provided on the command line are more powerful than, and will override, formatting options provided through the _mdfmt configuration files_.

Use of _mdfmt configuration files_ is optional.  If there are no _configuration files_, then the [target directory](#target-directory) is used as the [processing root](#processing-root), and only command line options govern the formatting that happens.

## processing root

a directory that defines the root of files that [Mdfmt](#mdfmt) can see and process.  [Mdfmt](#mdfmt) can only format `.md` files that are in the _processing root_ directory and its subdirectories.

It is always the case that the _processing root_ either is the [target directory](#target-directory) or an ancestor directory of the [target directory](#target-directory).  Starting in the [target directory](#target-directory) and scanning up the file system to successive ancestor directories, the _processing root_ is the first directory that contains any [mdfmt configuration files](#mdfmt-configuration-files).

## slug

in the context of [Mdfmt](#mdfmt), a string that can be used as a [destination](#destination) of a [link](#link), to navigate to a [Markdown](#markdown) document, often to a specific section of that document.  The _slug_ is based on the target file name and heading text, transformed by a function that does things like remove non-URL-safe characters, replace space by `-`, standardize casing, etc.  The specific [slugification](#slugification) rules used vary by the target [Markdown](#markdown) rendering platform.

## slugification

in the context of [Mdfmt](#mdfmt), the process of converting a [Markdown](#markdown) file name and the text of a heading the way it displays to the user, to a [destination](#destination) that can be used in a [link](#link).  There are different slugification algorithms that support different [Markdown](#markdown) rendering environments.  For example, [Markdown rendered on GitHub](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax) requires a different format of [slug](#slug) than [Azure DevOps Wiki](https://learn.microsoft.com/en-us/azure/devops/project/wiki).

## table of contents

a region of a [Markdown](#markdown) document that [Mdfmt](#mdfmt) maintains.  It comprises a hierarchical outline of [links](#link), where each [destination](#destination) goes to a heading of the document.

## target path

the path that [Mdfmt](#mdfmt) is being asked to process, optionally specified on the command line as either an absolute path or relative path.  When omitted from the command line, it defaults to the [current working directory](#current-working-directory).  When a relative path, it is relative to the [current working directory](#current-working-directory).  The _target path_ indicates either an individual [Markdown](#markdown) (`.md`) file or a directory.  If an individual file, it means only that file should be processed.  If a directory, it means to process the files in that directory, and in subdirectories too if the `-r --recursive` option is specified.

## target directory

The directory of the [target path](#target-path):

- If the [target path](#target-path) indicates a specific [Markdown](#markdown) (`.md`) file, then the _target directory_ is the directory containing this specific file.
- If the [target path](#target-path) indicates a directory, then the _target directory_ and [target path](#target-path) are one and the same.

## TOC

short for [table of contents](#table-of-contents).

## TOC threshold

an integer greater than or equal to 0, indicating the minimum number of headings that a document must have before a [TOC](#toc) is generated.  A _TOC threshold_ of 0 causes an existing table of contents to be removed.
