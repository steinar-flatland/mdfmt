# Introduction To Mdfmt

<!--BEGIN_TOC-->
- [Introduction To Mdfmt](#introduction-to-mdfmt)
  - [1. Motivation](#1-motivation)
  - [2. Approach](#2-approach)
  - [3. What Mdfmt Does To Your Markdown Files](#3-what-mdfmt-does-to-your-markdown-files)
  - [4. Next Ideas](#4-next-ideas)
<!--END_TOC-->

## 1. Motivation

Mdfmt is a Markdown formatter command line interface (CLI).  It was motivated by the following problems seen during development of a commercial documentation repository.

- A VS Code extension was being used for table of contents (TOC) generation.  The clickable links in the resulting TOC either worked in Visual Studio Code on the developer laptop, or in Azure DevOps wikis, but not both.  While the VS Code extension did support various "slugification" types to format for different platforms, this functionality was inconvenient to use at scale:
  - The slugification for Azure produced big ugly links, not readable to a human, which is counter to the spirit of Markdown.  
  - The extension did not keep links outside of the the Table of Contents up to date, for example, when one section of a document refers to another.
  - When working on documentation locally, and wanting to navigate by clicking, it was inconvenient to reformat the Markdown files one-by-one.
  - The need to support an increasing number of platforms, not just Azure Devops wikis and VS Code was anticipated.  Other possible platforms include GitHub, a Backstage software catalog, etc.

There was a desire for:

- Tooling to make it easier to deal with links and tables of contents when writing Markdown articles.
- A CLI driven approach for integrating Markdown formatting adjustments with CI/CD pipelines that are deploying documentation.

With these things in mind, Mdfmt was born.  Mdfmt today is young, thinly featured, and brimming with potential!

## 2. Approach

Mdfmt is a CLI.  You can apply it to a single Markdown file or to a folder that contains Markdown files.  When applied to a folder, it can optionally use recursion to process Markdown files in subfolders too.  If you are familiar with Terraform development and have used commands like `terraform fmt` or `tofu fmt` to rewrite Terraform configuration files to a canonical format and style, Mdfmt is a similar type of tool.  It is a CLI that helps you format Markdown files under a common root.

## 3. What Mdfmt Does To Your Markdown Files

> Some Background Info:  A Markdown link is of the form `[label](destination)`.  For an in-document link to a section, the `label` typically matches the heading text of the section.  The `destination` is a way of encoding the target of the link.  For external links the `destination` can be a full URL.  For in-document links, the `destination` can be expressed simply as a fragment like `#my-heading` or it can also include path information like `./My-File.md#my-heading`.  For in-document links, whether to include the `./My-file` part or not can influence whether the platform interpreting and displaying the Markdown will be able to resolve the link, and strangely, the choice can be related to whether the heading contains punctuation.  When headings include punctuation, incompatibilities between platforms quickly arise.  Different platforms have different requirements about whether to omit the punctuation or leave it or encode it, and whether the destination needs to include a path part or not.

The initial version of Mdfmt does the following things when it runs:

1. Adds or updates a table of contents, formatted for a target platform (Azure or VsCode).
2. Ensures that the `destination` of each in-document link is up-to-date with the intent expressed in its `label`, formatted according to the target platform.  Only links where the `destination` exactly matches a heading are updated to target navigation to that heading.

Optionally, Mdfmt can be used to reformat Markdown files to enforce a given newline style (unix vs. Windows).  Mdfmt can also work in a non-opinionated way, where it respects the line endings in a file, and forces them over to a preferred style only when the line endings within the file are inconsistent.

Its a modest start, but it shows the potential.

## 4. Next Ideas

Upcoming features include:

- Allow users to edit headings, without needing to worry about making conforming changes to the TOC or to links in the document that target the edited heading.  Mdfmt will retarget both links in the TOC and links in the document to the edited heading.  This approach leverages a stale TOC as a sort of memory of how headings used to be, providing a pathway for noticing stale in-document links and directing them over to a recently edited heading.
- Letting Mdfmt take charge of assigning and reassigning hierarchical section numbering.  When using section numbers, this avoids the pain of manually renumbering a bunch of sections because a section got inserted in the middle.
- ...
