using Mdfmt.Generators.Links;
using Mdfmt.Model;
using Mdfmt.Utilities;
using System.Collections.Generic;
using System.Text;

namespace Mdfmt.Generators.Tocs;

internal class TocGenerator(ILinkDestinationGenerator linkDestinationGenerator)
{
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;

    public Toc GenerateToc(string filename, List<Heading> headings, string newlineSequence)
    {
        StringBuilder sb = new();
        sb.Append(Markers.BeginTocMarker);
        sb.Append(newlineSequence);
        foreach (Heading heading in headings)
            GenerateTocEntry(sb, filename, heading, newlineSequence);
        sb.Append(Markers.EndTocMarker);
        return new Toc(sb.ToString(), headings.Count);
    }

    public void GenerateTocEntry(StringBuilder sb, string filename, Heading heading, string newlineSequence)
    {
        int spaceCount = heading.Level * 2;
        sb.Append(' ', spaceCount);
        sb.Append("- [");
        sb.Append(heading.Text);
        sb.Append("](");
        sb.Append(_linkDestinationGenerator.GenerateLinkDestination(filename, heading.Text));
        sb.Append(')');
        sb.Append(newlineSequence);
    }
}
