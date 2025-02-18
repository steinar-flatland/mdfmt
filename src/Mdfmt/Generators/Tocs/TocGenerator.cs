using Mdfmt.Generators.Links;
using Mdfmt.Model;
using Mdfmt.Utilities;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mdfmt.Generators.Tocs;

internal class TocGenerator(ILinkDestinationGenerator linkDestinationGenerator)
{
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;

    public Toc GenerateToc(string filename, IEnumerable<HeadingRegion> headings, string newline)
    {
        StringBuilder sb = new();
        sb.Append(Markers.BeginTocMarker);
        sb.Append(newline);
        foreach (HeadingRegion heading in headings)
            GenerateTocEntry(sb, filename, heading, newline);
        sb.Append(Markers.EndTocMarker);
        return new Toc(sb.ToString(), headings.Count());
    }

    public void GenerateTocEntry(StringBuilder sb, string filename, HeadingRegion headingRegion, string newlineSequence)
    {
        int spaceCount = headingRegion.Level * 2;
        sb.Append(' ', spaceCount);
        sb.Append("- [");
        sb.Append(headingRegion.HeadingText);
        sb.Append("](");
        sb.Append(_linkDestinationGenerator.GenerateInDocumentLinkDestination(filename, headingRegion.HeadingText));
        sb.Append(')');
        sb.Append(newlineSequence);
    }
}
