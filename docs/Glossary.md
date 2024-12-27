# Glossary

<!--BEGIN_TOC-->
- [Glossary](#glossary)
  - [cpath](#cpath)
  - [destination](#destination)
  - [label](#label)
  - [link](#link)
  - [Markdown](#markdown)
  - [Mdfmt](#mdfmt)
  - [slug](#slug)
  - [slugification](#slugification)
<!--END_TOC-->

This file defines terms that form the ubiquitous language of Mdfmt.

## cpath

the canonical relative path of a file being processed by [Mdfmt](#mdfmt).

If [Mdfmt](#mdfmt) is processing a single file, the _cpath_ is the string `./` prepended to the name of the file.  For example, if a single file named `MyFile.md` is being processed, its `cpath` is `./MyFile.md`.

If [Mdfmt](#mdfmt) is processing all of the Markdown files under a specified directory, the _cpath_ of each file being processed is a relative path rooted at `./` where `.` represents the directory being processed.  For example, if the directory being processed has a subfolder, `Foo`, that contains a file being processed, and this file is named `MyFile.md`, then the _cpath_ when processing this file is `./Foo/MyFile.md`.

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

## Mdfmt

a CLI tool for formatting either individual [Markdown](#markdown) files or all the [Markdown](#markdown) files found in a directory.

## slug

in the context of [Mdfmt](#mdfmt), a string that can be used as a [destination](#destination) of a [link](#link), to navigate to a [Markdown](#markdown) document, often to a specific section of that document.  The _slug_ is based on the target file name and heading text, transformed by a function that does things like remove non-URL-safe characters, replace space by `-`, standardizing casing, etc.  The specific [slugification](#slugification) rules used vary by the target [Markdown](#markdown) rendering platform.

## slugification

in the context of [Mdfmt](#mdfmt), the process of converting a [Markdown](#markdown) file name and the text of a heading the way it displays to the user, to a [destination](#destination) that can be used in a [link](#link).  There are different slugification algorithms that support different [Markdown](#markdown) rendering environments.  For example, [Markdown rendered on GitHub](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax) requires a different format of slug than [Azure DevOps Wiki](https://learn.microsoft.com/en-us/azure/devops/project/wiki).
