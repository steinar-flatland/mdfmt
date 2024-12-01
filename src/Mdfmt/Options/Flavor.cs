namespace Mdfmt.Options;

/// <summary>
/// Enumerated type defining flavors of Markdown formatting.
/// </summary>
internal enum Flavor
{
    /// <summary>
    /// Common flavor of formatting, that works with GitHub and VsCode Markdown preview.
    /// </summary>
    Common,

    /// <summary>
    /// Azure DevOps Wiki.
    /// </summary>
    Azure
}
