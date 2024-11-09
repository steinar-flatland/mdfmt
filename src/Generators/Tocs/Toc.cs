namespace Mdfmt.Generators.Tocs;

internal class Toc(string value, int entryCount)
{
    /// <summary>
    /// The Markdown content of the table of contents.
    /// </summary>
    public string Content { get; } = value;

    /// <summary>
    /// The number of entries in the table of contents.
    /// </summary>
    public int EntryCount { get; } = entryCount;
}
