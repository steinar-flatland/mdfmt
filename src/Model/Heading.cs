using System.Text.Json;

namespace Mdfmt.Model;

/// <summary>
/// A Markdown heading.
/// </summary>
internal class Heading(string cpath, int level, string text)
{
    /// <summary>
    /// Canonical relative file path, or cpath.
    /// </summary>
    public string Cpath { get; } = cpath;

    /// <summary>
    /// The level of the heading, which is an integer one less than the number of '#' characters in
    /// the Markdown where the heading comes from.
    /// </summary>
    public int Level { get; } = level;

    /// <summary>
    /// The text of the heading, cleaned up for how it will be displayed.  This means removing
    /// leading '#' characters, removing any embedded HTML comments if part of the heading is
    /// commented out, and trimming any leading or trailing spaces.
    /// </summary>
    public string Text { get; } = text;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
