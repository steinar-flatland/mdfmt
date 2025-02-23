namespace Mdfmt.Model;

/// <summary>
/// Enumerated type for classifying links.
/// </summary>
internal enum LinkType
{
    /// <summary>
    /// A link that goes between sections of the same docuemnt.
    /// </summary>
    InDocument,

    /// <summary>
    /// A link that goes between documents using a (possibly empty) relative path.
    /// </summary>
    CrossDocument,

    /// <summary>
    /// A link that is not one of the other types.  Typically a hyperlink (http, https, etc.)
    /// </summary>
    External
}
