using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using System;

namespace Mdfmt;

/// <summary>
/// Perform needed updates to the Regions data structure, into which the Markdown file has been loaded.
/// </summary>
/// <param name="tocGenerator">Table of contents generator</param>
/// <param name="minimumEntryCount">The minimum number of entries in a Table of Contents before it will be added to a file</param>
/// <param name="linkDestinationGenerator">Generator for link destinations, to update links in the document</param>
/// <param name="verbose">Whether to generate extra output about what the program is doing</param>
public class Updater(TocGenerator tocGenerator, int minimumEntryCount, ILinkDestinationGenerator linkDestinationGenerator, bool verbose)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;
    private readonly int _minimumEntryCount = minimumEntryCount;
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;
    private readonly bool _verbose = verbose;

    private MdStruct _md;

    public void Update(MdStruct md)
    {
        _md = md;
        UpdateToc();
        UpdateLinks();
    }

    private void UpdateToc()
    {
        // Generate a TOC based on headings.
        Toc newToc = _tocGenerator.GenerateToc(_md.FileName, _md.HeadingRegions, _md.Newline);

        // Is the desired end state to have a TOC with the content of newToc in the document?
        bool tocShouldExist =
            _minimumEntryCount > 0 &&
            newToc.EntryCount >= _minimumEntryCount;

        if (_md.HasToc) // The document has a TOC already.
        {
            if (tocShouldExist)
            {
                Region tocRegion = _md.TocRegion;
                if (tocRegion.Content != newToc.Content)
                {
                    tocRegion.Content = newToc.Content;
                    if (_verbose)
                    {
                        Console.WriteLine("  Updated TOC");
                    }
                }
            }
            else
            {
                _md.DeleteToc();
                Console.WriteLine("  Removed TOC");
            }
        }
        else // The document does not have a TOC.
        {
            if (tocShouldExist)
            {
                _md.AddToc(newToc.Content);
                if (_verbose)
                {
                    Console.WriteLine("  Inserted new TOC");
                }
            }
        }
    }

    private void UpdateLinks()
    {
        foreach (LinkRegion linkRegion in _md.LinkRegions)
        {
            if (LinkWithinSameFile(linkRegion.Destination))
            {
                if (_md.TryGetHeadingRegion(linkRegion.Label, out HeadingRegion headingRegion) ||
                    _md.TryGetHeadingRegion(linkRegion.Destination, out headingRegion))
                {
                    string destination = _linkDestinationGenerator.GenerateLinkDestination(_md.FileName, headingRegion.HeadingText);
                    if (linkRegion.Destination != destination)
                    {
                        linkRegion.Destination = destination;
                        if (_verbose)
                        {
                            Console.WriteLine($"  Updated link with label [{linkRegion.Label}] to target destination ({linkRegion.Destination})");
                        }
                    }
                }
                else
                {
                    if (_verbose)
                    {
                        Console.WriteLine($"  Could not match link to heading: {linkRegion.Content}");
                    }
                }
            }
        }
    }

    private bool LinkWithinSameFile(string destination)
    {
        return string.IsNullOrEmpty(destination) || destination.StartsWith('#') || destination.StartsWith($"./{_md.FileName}");
    }
}
