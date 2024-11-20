using Mdfmt.Generators.Links;
using Mdfmt.Model;
using Mdfmt.Options;
using System;

namespace Mdfmt.Updaters;

public class LinkUpdater(CommandLineOptions options, ILinkDestinationGenerator linkDestinationGenerator) : UpdaterBase(options)
{
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;

    public override void Update(MdStruct md)
    {
        foreach (LinkRegion linkRegion in md.LinkRegions)
        {
            bool linkIsWithinSameFile =
                string.IsNullOrEmpty(linkRegion.Destination) ||
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
                        if (_options.Verbose)
                        {
                            Console.WriteLine($"  Updated link with label [{linkRegion.Label}] to target destination ({linkRegion.Destination})");
                        }
                    }
                }
                else
                {
                    if (_options.Verbose)
                    {
                        Console.WriteLine($"  Could not match link to heading: {linkRegion.Content}");
                    }
                }
            }
        }
    }
}
