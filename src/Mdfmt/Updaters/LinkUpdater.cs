using Mdfmt.Generators.Links;
using Mdfmt.Model;

namespace Mdfmt.Updaters;

internal class LinkUpdater(ILinkDestinationGenerator linkDestinationGenerator)
{
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;

    public void Update(MdStruct md, bool verbose)
    {
        foreach (LinkRegion linkRegion in md.LinkRegions)
        {
            bool linkIsWithinSameFile =
                string.IsNullOrWhiteSpace(linkRegion.Destination) ||
                linkRegion.Destination.StartsWith('#') ||
                linkRegion.Destination.StartsWith($"./{md.FileName}");
            if (linkIsWithinSameFile)
            {
                if (md.TryGetHeadingRegion(linkRegion.Label, out HeadingRegion headingRegion) ||
                    md.TryGetHeadingRegion(linkRegion.Destination, out headingRegion))
                {
                    string destination = _linkDestinationGenerator.GenerateLinkDestination(md.FileName, headingRegion.HeadingText);
                    if (linkRegion.Destination != destination)
                    {
                        linkRegion.Destination = destination;
                        if (verbose)
                        {
                            Output.Emphasis($"Updated link with label [{linkRegion.Label}] to target destination ({linkRegion.Destination})");
                        }
                    }
                }
                else
                {
                    if (verbose)
                    {
                        Output.Warn($"Could not match link to heading: {linkRegion.Content}");
                    }
                }
            }
        }
    }
}
