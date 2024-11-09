using Mdfmt.Model;
using Mdfmt.Utilities;
using System.Collections.Generic;

namespace Mdfmt.Loaders;

/// <summary>
/// <para>
/// Parser for a line of Markdown.  Apply this subordinate parser when the main parser is in 
/// Parser.Normal state, and it has been determined that the current line is not a table of
/// contents, fenced code block, or line in an indented code block.  In short, by process of
/// elimination, when there's no other special way to handle the current line, then use this
/// line parser.  Used in this way, this parser will apply to lines of headings and paragraphs
/// in the Markdown.
/// </para>
/// <para>
/// This parser parses the line into a list of 1 or more AtomicRegions.  (Note: A completely
/// empty line generates a single empty AtomicRegion.)
/// </para>
/// </summary>
internal static class NormalLineParser
{
    /// <summary>
    /// <para>
    /// Parse the line into a list of 1 or more AtomicRegions.
    /// </para>
    /// <para>
    /// Content that is commented out in the Markdown is placed into immutable AtomicRegions, and
    /// content that is not commented out in the Markdown goes into mutable AtomicRegions (see
    /// IsMutable of AtomicRegion).
    /// </para>
    /// <para>
    /// If an AtomicRegion holds an ENTIRE Markdown heading, then its IsHeading property is set
    /// to true.  If an AtomicRegion holds only a portion of a heading and not the entire heading
    /// then IsHeading is false.  (The idea is that if a heading is spread across multiple atomic
    /// regions, higher level code will incorporate these regions into a containing CompositeRegion
    /// that has IsHeading==true.)
    /// </para>
    /// </summary>
    /// <param name="inHtmlComment">Whether in an HTML comment when this method is called.</param>
    /// <param name="line">The line to parse.</param>
    /// <returns>
    /// NormalLineParseResult
    /// </returns>
    public static NormalLineParseResult Parse(bool inHtmlComment, string line)
    {
        int index = 0; // Start index of region being parsed.
        int endIndex;  // End index of region being parsed.

        // Build list of regions.
        List<AtomicRegion> regions = [];
        while (index < line.Length)
            if (!inHtmlComment)
            {
                endIndex = line.IndexOf(Markers.HtmlCommentStart, index);
                if (endIndex < 0)
                {
                    AtomicRegion atomicRegion = new(line[index..], isMutable: true);
                    regions.Add(atomicRegion);
                    break;
                }
                else
                {
                    if (endIndex - index > 0) // Avoids useless empty region when comment starts at start of line.
                    {
                        AtomicRegion atomicRegion = new(line[index..endIndex], isMutable: true);
                        regions.Add(atomicRegion);
                    }
                    index = endIndex;
                    inHtmlComment = true;
                }
            }
            else // in HTML comment
            {
                endIndex = line.IndexOf(Markers.HtmlCommentEnd, index);
                if (endIndex < 0)
                {
                    AtomicRegion atomicRegion = new(line[index..], isMutable: false);
                    regions.Add(atomicRegion);
                    break;
                }
                else
                {
                    // Grow the commented region to encompass the end of comment marker.
                    endIndex += Markers.HtmlCommentEnd.Length;

                    AtomicRegion atomicRegion = new(line[index..endIndex], isMutable: false);
                    regions.Add(atomicRegion);
                    index = endIndex;
                    inHtmlComment = false;
                }
            }

        // If there is a single mutable region starting with '#', repackage the AtomicRegion indicating that it is a heading.
        if (regions.Count == 1 && regions[0].IsMutable && regions[0].Content.TrimStart().StartsWith('#'))
        {
            // AtomicRegion has IsHeading == false.  It needs to be repackaged.
            AtomicRegion region = regions[0];
            regions.Clear();
            regions.Add(new AtomicRegion(region.Content, region.IsMutable, isHeading: true));
        }

        // A completely empty line parses into 0 regions.  Subtle, but we need to add a single empty region in this
        // case, otherwise, the data structure coming from the parse is losing information about how many lines
        // the original file had.
        if (regions.Count == 0)
            regions.Add(new AtomicRegion(string.Empty, isMutable: !inHtmlComment));

        return new NormalLineParseResult(inHtmlComment, regions);
    }
}
