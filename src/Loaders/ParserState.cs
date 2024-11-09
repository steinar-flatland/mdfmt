namespace Mdfmt.Loaders;

/// <summary>
/// States of a Markdown parser.
/// </summary>
internal enum ParserState
{
    /// <summary>
    /// Initial state of parser. In this state it processes lines with headings, paragraphs, etc.
    /// or it switches to one of the other states when special content is seen.
    /// </summary>
    Normal,

    /// <summary>
    /// Table of contents that Mdfmt previously added, enclosed by <!--BEGIN_TOC--> and <!--END_TOC-->.
    /// </summary>
    InTableOfContents,

    /// <summary>
    /// Fenced code block enclosed in starting and ending ```.
    /// </summary>
    InFencedCodeBlock,

    /// <summary>
    /// Inside a multi-line HTML comment.
    /// </summary>
    InHtmlComment
}
