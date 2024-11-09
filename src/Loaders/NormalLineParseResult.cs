using Mdfmt.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Mdfmt.Loaders;

/// <summary>
/// The result of applying the <see cref="NormalLineParser"/>.
/// </summary>
internal readonly struct NormalLineParseResult(bool inHtmlComment, IReadOnlyList<AtomicRegion> atomicRegions)
{
    /// <summary>
    /// Whether in an HTML comment after parsing the line.  This is important, because comments can
    /// span lines, and we need to keep track of whether currently in an HTML comment.
    /// </summary>
    public bool InHtmlComment { get; } = inHtmlComment;

    /// <summary>
    /// List of 1 or more AtomicRegions, in the same order they were parsed from the line.
    /// (Note that an empty line parses into one empty AtomicRegion.)
    /// </summary>
    public IReadOnlyList<AtomicRegion> AtomicRegions { get; } = atomicRegions;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Returns the concatenation of the content of all mutable atomic regions.  This will eliminate
    /// content that is commented out, and it will keep the active (uncommented) content.  If there
    /// are no mutable atomic regions, returns empty list.
    /// </summary>
    public string MutableContent => string.Concat(AtomicRegions.Where(region => region.IsMutable).Select(region => region.Content));

    /// <summary>
    /// Determine whether the uncommented portion of the parsed line content has the content for
    /// a Markdown heading.
    /// </summary>
    public bool IsHeading => MutableContent.TrimStart().StartsWith('#');
}
