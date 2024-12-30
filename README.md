# Mdfmt

## Summary

Mdfmt is a command line interface (CLI) for Markdown formatting.  While it was initially developed to support software documentation development, it is a general purpose tool that is also applicable to other use cases.  Able to operate on individual Markdown files or on directory structures containing many Markdown files, it offers assistance with automated maintenance of heading numbers, in-document links, table of contents, and consistent newlines.  It is a work in progress, and new features continue to be added.

Mdfmt is being developed in C#/.NET 8.0, with plans to upgrade to the next LTS version of .NET when it comes out.

## Features and Benefits

- Get an overview of command line options with `mdfmt --help`.
- Format one file at a time or format all of the Markdown files recursively within a directory.
- CLI approach is convenient both for interactive use by authors/developers and for automated calls in publishing pipelines to ensure consistent formatting.
- The following formatting features are supported:
  - Generates and maintains section numbers in headings automatically.  In complex documents with nested sections, section numbers are important to help readers understand the hierarchical structure of the document.  Section numbers in Markdown documents are difficult for authors to maintain manually, and Mdfmt provides helpful automation, calculating and recalculating section numbers automatically as new sections are added between preexisting ones.
  - Maintains in-document links as section numbers change, with support for a couple of different ways of [slugifying](./docs/user/Glossary.md#slugification) headings.
  - Optionally autogenerates table of contents based on document headings.  The resulting table of contents allows navigation to each section of the document.
  - Supports different strategies for managing newline characters in Markdown files, which can be tricky in a cross-platform environment.
- When working on a documentation repository, or more generally any time there is a directory structure containing Markdown files, supports creation of a `.mdfmt` profile that specifies how to format the Markdown files in the directory.  The profile supports an inheritance scheme where you can specify in general how to format the Markdown, and override the general behavior in specific subdirectories or even individual files.  For example, maybe you prefer heading numbers in general, but not in a document that is a glossary containing a flat list of terms, where the heading numbers would be distracting not helpful.

## Contact

If you would like to get in touch with me about Mdfmt, please feel free to reach out to <steinar.flatland+mdfmt@gmail.com>.
