namespace Mdfmt.Model;

/// <summary>
/// Enumerated type for classifying links.
/// </summary>
internal enum LinkType
{
    /// <summary>
    /// A link that goes between sections of the same Markdown document.  (Note that collectively,
    /// we refer to <c>InDocuemnt</c> and <c>CrossDocument</c> links as internal links.)
    /// </summary>
    InDocument,

    /// <summary>
    /// A link that goes between two Markdown documents that occur under the same processing root.
    /// (Note that collectively, we refer to <c>InDocuemnt</c> and <c>CrossDocument</c> links as
    /// internal links.)
    /// </summary>
    CrossDocument,

    /// <summary>
    /// A link that is not one of the other types.  Typically a hyperlink (http, https).
    /// It could also be a link from a Markdown document to some other type of document.
    /// </summary>
    External
}
