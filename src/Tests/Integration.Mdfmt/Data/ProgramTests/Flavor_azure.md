# Flavor Test

<!--BEGIN_TOC-->
- [Flavor Test](#flavor-test)
  - [1. Introduction](./file.md#1.-introduction)
  - [2. Changes When Converting Between Flavors](./file.md#2.-changes-when-converting-between-flavors)
    - [2.1. Format of In-Document Links Recongized By Mdfmt](./file.md#2.1.-format-of-in-document-links-recongized-by-mdfmt)
    - [2.2. Examples of In-Document Links](./file.md#2.2.-examples-of-in-document-links)
  - [3. Not Changed When Converting Betwen Flavors](./file.md#3.-not-changed-when-converting-betwen-flavors)
    - [3.1. Links In Single Backtick Regions](./file.md#3.1.-links-in-single-backtick-regions)
    - [3.2. Fenced Code Regions](./file.md#3.2.-fenced-code-regions)
      - [3.2.1. Example Of TOC Using Common Flavor](./file.md#3.2.1.-example-of-toc-using-common-flavor)
      - [3.2.2. Example Of TOC Using Azure Flavor](./file.md#3.2.2.-example-of-toc-using-azure-flavor)
    - [3.3. Indented Code Regions](./file.md#3.3.-indented-code-regions)
      - [3.3.1. Example Of TOC Using Common Flavor](./file.md#3.3.1.-example-of-toc-using-common-flavor)
      - [3.3.2. Example Of TOC Using Azure Flavor](./file.md#3.3.2.-example-of-toc-using-azure-flavor)
<!--END_TOC-->

## 1. Introduction

This test is designed to test differences between the `common` and `azure` flavors of Markdown.

There are two files:

1. `Flavor_common.md`
2. `Flavor_azure.md`

The test is successful when Mdfmt converts each file to the other using the `-f` option:

- `mdfmt -f Azure Flavor_common.md`
  - The above command converts the `Flavor_common.md` file so it is exactly the same as `Flavor_azure.md`.
- `mdfmt -f Common Flavor_azure.md`
  - The above command converts the `Flavor_azure.md` file so it is exactly the same as `Flavor_common.md`.

## 2. Changes When Converting Between Flavors

When converting between Markdown flavors, Mdfmt changes the following things:

1. Format of in-document links in table of contents.
2. Format of in-document links throughout the document.

At this time, Mdfmt does not make changes to any links that are referencing other documents.  (Maintaining cross-document linkages within the same root directory of files being processed is a possible future feature.)

### 2.1. Format of In-Document Links Recongized By Mdfmt

A link is a string of the form `[label](destination)`.  The `label` part is text that is displayed to the user as part of the document, and the destination indicates the target document and heading.

An in-document link goes to a heading of the same document.  Mdfmt recognizes a link as in-document if any one of the following three things is true:

1. The destination of the link starts with the character `'#'`.
2. The destination of the link starts with `./{fileName.md}` where `{fileName.md}` is the simple file name of the file where the link occurs, e.g. `Flavor_common.md` or `Flavor_azure.md`.
3. The destination of the link is an empty string or consists entirely of whitespace characters. (Then Mdfmt will try to populate the destination by finding a heading with text that matches the link's label.  This aspect is out of scope of this Flavor test.)

### 2.2. Examples of In-Document Links

Here are some examples of links to other sections of this document.

This document has sections showing examples of both fenced and indented code regions.  See [3.2. Fenced Code Regions](./file.md#3.2.-fenced-code-regions) and [3.3. Indented Code Regions](./file.md#3.3.-indented-code-regions).

These links _should_ be maintained as part of switching Markdown flavors.

Note that the links will be maintained, even if the label has been changed to no longer exactly match the target heading:

- [Fenced Code](./file.md#3.2.-fenced-code-regions)
- [Indented Code](./file.md#3.3.-indented-code-regions)

## 3. Not Changed When Converting Betwen Flavors

When changing between flavors, Mdfmt changes only the things discussed in the previous section.  This section discusses a non-exhaustive list of some notable things that Mdfmt does _not_ change when converting between flavors.

Mdmft does not change:

1. Links in single backtick regions
2. Fenced code regions
3. Indented code regions

### 3.1. Links In Single Backtick Regions

Note that what would otherwise be links within the document, if inside a single backtick region, will not be reformatted by Mdfmt when a different flavor is applied.  Mdfmt does not edit single backtick regions.

Here's an example of Markdown for a link to the Introduction section above, using the `Common` flavor:  `[1. Introduction](#1-introduction)`.

Here's an example of Markdown for a link to the Introduction section above, using the `Azure` flavor:  `[1. Introduction](./Flavor_common.md#1.-introduction)`.

### 3.2. Fenced Code Regions

A fenced code region is enclosed in triple backticks.  Mdfmt does not edit fenced code regions.

#### 3.2.1. Example Of TOC Using Common Flavor

The following Markdown shows the table of contents of this document, expressed using the Common Markdown flavor.  Mdfmt does not update this fenced code region when changing flavors.

```Markdown
<!--BEGIN_TOC-->
- [Flavor Test](#flavor-test)
  - [1. Introduction](#1-introduction)
  - [2. Changes When Converting Between Flavors](#2-changes-when-converting-between-flavors)
    - [2.1. Format of In-Document Links Recongized By Mdfmt](#21-format-of-in-document-links-recongized-by-mdfmt)
    - [2.2. Examples of In-Document Links](#22-examples-of-in-document-links)
  - [3. Not Changed When Converting Betwen Flavors](#3-not-changed-when-converting-betwen-flavors)
    - [3.1. Single Backtick Regions](#31-single-backtick-regions)
    - [3.2. Fenced Code Regions](#32-fenced-code-regions)
      - [3.2.1. Example Of TOC Using Common Flavor](#321-example-of-toc-using-common-flavor)
      - [3.2.2. Example Of TOC Using Azure Flavor](#322-example-of-toc-using-azure-flavor)
    - [3.3. Indented Code Regions](#33-indented-code-regions)
      - [3.3.1. Example Of TOC Using Common Flavor](#331-example-of-toc-using-common-flavor)
      - [3.3.2. Example Of TOC Using Azure Flavor](#332-example-of-toc-using-azure-flavor)
<!--END_TOC-->
```

#### 3.2.2. Example Of TOC Using Azure Flavor

The following Markdown shows the table of contents of this document, expressed using the Azure Markdown flavor.  Mdfmt does not update this fenced code region when changing flavors.

```Markdown
<!--BEGIN_TOC-->
- [Flavor Test](#flavor-test)
  - [1. Introduction](./Flavor_common.md#1.-introduction)
  - [2. Changes When Converting Between Flavors](./Flavor_common.md#2.-changes-when-converting-between-flavors)
    - [2.1. Format of In-Document Links Recongized By Mdfmt](./Flavor_common.md#2.1.-format-of-in-document-links-recongized-by-mdfmt)
    - [2.2. Examples of In-Document Links](./Flavor_common.md#2.2.-examples-of-in-document-links)
  - [3. Not Changed When Converting Betwen Flavors](./Flavor_common.md#3.-not-changed-when-converting-betwen-flavors)
    - [3.1. Single Backtick Regions](./Flavor_common.md#3.1.-single-backtick-regions)
    - [3.2. Fenced Code Regions](./Flavor_common.md#3.2.-fenced-code-regions)
      - [3.2.1. Example Of TOC Using Common Flavor](./Flavor_common.md#3.2.1.-example-of-toc-using-common-flavor)
      - [3.2.2. Example Of TOC Using Azure Flavor](./Flavor_common.md#3.2.2.-example-of-toc-using-azure-flavor)
    - [3.3. Indented Code Regions](./Flavor_common.md#3.3.-indented-code-regions)
      - [3.3.1. Example Of TOC Using Common Flavor](./Flavor_common.md#3.3.1.-example-of-toc-using-common-flavor)
      - [3.3.2. Example Of TOC Using Azure Flavor](./Flavor_common.md#3.3.2.-example-of-toc-using-azure-flavor)
<!--END_TOC-->
```

### 3.3. Indented Code Regions

An indented code region is indented from the left margin by at least 4 spaces or 1 tab.  Mdfmt does not edit indented code regions.

#### 3.3.1. Example Of TOC Using Common Flavor

The following Markdown shows the table of contents of this document, expressed using the Common Markdown flavor.  The TOC is presented using an indented code region, indented from the left margin by 4 spaces.  Mdfmt does not update this indented code region when changing flavors.

    <!--BEGIN_TOC-->
    - [Flavor Test](#flavor-test)
      - [1. Introduction](#1-introduction)
      - [2. Changes When Converting Between Flavors](#2-changes-when-converting-between-flavors)
        - [2.1. Format of In-Document Links Recongized By Mdfmt](#21-format-of-in-document-links-recongized-by-mdfmt)
        - [2.2. Examples of In-Document Links](#22-examples-of-in-document-links)
      - [3. Not Changed When Converting Betwen Flavors](#3-not-changed-when-converting-betwen-flavors)
        - [3.1. Single Backtick Regions](#31-single-backtick-regions)
        - [3.2. Fenced Code Regions](#32-fenced-code-regions)
          - [3.2.1. Example Of TOC Using Common Flavor](#321-example-of-toc-using-common-flavor)
          - [3.2.2. Example Of TOC Using Azure Flavor](#322-example-of-toc-using-azure-flavor)
        - [3.3. Indented Code Regions](#33-indented-code-regions)
          - [3.3.1. Example Of TOC Using Common Flavor](#331-example-of-toc-using-common-flavor)
          - [3.3.2. Example Of TOC Using Azure Flavor](#332-example-of-toc-using-azure-flavor)
    <!--END_TOC-->

#### 3.3.2. Example Of TOC Using Azure Flavor

The following Markdown shows the table of contents of this document, expressed using the Azure Markdown flavor.  The TOC is presented using an indented code region, indented from the left margin by 1 tab.  Mdfmt does not update this indented code region when changing flavors.

	<!--BEGIN_TOC-->
	- [Flavor Test](#flavor-test)
	  - [1. Introduction](./Flavor_common.md#1.-introduction)
	  - [2. Changes When Converting Between Flavors](./Flavor_common.md#2.-changes-when-converting-between-flavors)
	    - [2.1. Format of In-Document Links Recongized By Mdfmt](./Flavor_common.md#2.1.-format-of-in-document-links-recongized-by-mdfmt)
	    - [2.2. Examples of In-Document Links](./Flavor_common.md#2.2.-examples-of-in-document-links)
	  - [3. Not Changed When Converting Betwen Flavors](./Flavor_common.md#3.-not-changed-when-converting-betwen-flavors)
	    - [3.1. Single Backtick Regions](./Flavor_common.md#3.1.-single-backtick-regions)
	    - [3.2. Fenced Code Regions](./Flavor_common.md#3.2.-fenced-code-regions)
	      - [3.2.1. Example Of TOC Using Common Flavor](./Flavor_common.md#3.2.1.-example-of-toc-using-common-flavor)
	      - [3.2.2. Example Of TOC Using Azure Flavor](./Flavor_common.md#3.2.2.-example-of-toc-using-azure-flavor)
	    - [3.3. Indented Code Regions](./Flavor_common.md#3.3.-indented-code-regions)
	      - [3.3.1. Example Of TOC Using Common Flavor](./Flavor_common.md#3.3.1.-example-of-toc-using-common-flavor)
	      - [3.3.2. Example Of TOC Using Azure Flavor](./Flavor_common.md#3.3.2.-example-of-toc-using-azure-flavor)
	<!--END_TOC-->
