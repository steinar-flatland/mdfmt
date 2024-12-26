# Glossary

<!--BEGIN_TOC-->
- [Glossary](#glossary)
  - [cpath](#cpath)
  - [Markdown](#markdown)
  - [Mdfmt](#mdfmt)
<!--END_TOC-->

This file defines terms that form the ubiquitous language of Mdfmt.

## cpath

the canonical relative path of a file being processed by [Mdfmt](#mdfmt).

If [Mdfmt](#mdfmt) is processing a single file, the _cpath_ is the string `./` prepended to the name of the file.  For example, if a single file named `MyFile.md` is being processed, its `cpath` is `./MyFile.md`.

If [Mdfmt](#mdfmt) is processing all of the Markdown files under a specified directory, the _cpath_ of each file being processed is a relative path rooted at `./` where `.` represents the directory being processed.  For example, if the directory being processed has a subfolder, `Foo`, that contains a file being processed, and this file is named `MyFile.md`, then the _cpath_ when processing this file is `./Foo/MyFile.md`.

## Markdown

created by [John Gruber](https://daringfireball.net/contact/) in 2004, he describes _Markdown_ as a "text-to-HTML conversion tool for web writers" in this [introductory article about Markdown](https://daringfireball.net/projects/markdown/).  It comprises both a plain text syntax for adding formatting to an author's content, and a tool for converting this formatted plain text to HTML so it can be rendered.  Gruber describes the most important design goal for Markdown's formatting syntax in the above mentioned article:

> The overriding design goal for Markdown’s formatting syntax is to make it as readable as possible. The idea is that a Markdown-formatted document should be publishable as-is, as plain text, without looking like it’s been marked up with tags or formatting instructions.

Since its introduction, Markdown has been widely adopted.  There are numerous implementations with varying features and details.  The [Wikipedia article on Markdown](https://en.wikipedia.org/wiki/Markdown) provides more context, history, examples, and lists many of the implementations.

## Mdfmt

a CLI tool for formatting either individual [Markdown](#markdown) files or all the [Markdown](#markdown) files found in a directory.
