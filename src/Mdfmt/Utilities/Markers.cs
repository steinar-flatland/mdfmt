namespace Mdfmt.Utilities;

/// <summary>
/// Various landmarks useful when parsing or generating Markdown files.
/// </summary>
internal class Markers
{
    public static readonly string BeginTocMarker = "<!--BEGIN_TOC-->";
    public static readonly string EndTocMarker = "<!--END_TOC-->";
    public static readonly string FenceMarker = "```";
    public static readonly string FourSpaces = "    ";
    public static readonly string HtmlCommentStart = "<!--";
    public static readonly string HtmlCommentEnd = "-->";
}
