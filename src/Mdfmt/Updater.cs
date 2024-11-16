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

        // If the TOC is too small, get out.
        if (newToc.EntryCount < _minimumEntryCount)
        {
            return;
        }

        if (_md.HasToc)
        {
            // There is a TOC.  Update it if necessary.
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
            // There is no TOC.  If there is a first heading, insert it after that.
            if (_md.HasHeading)
            {
                _md.InsertTocAfterFirstHeading(newToc.Content);
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
                if (_md.TryGetHeadingRegion(linkRegion.Label, out HeadingRegion headingRegion))
                {
                    string destination = _linkDestinationGenerator.GenerateLinkDestination(_md.FileName, headingRegion.HeadingText);
                    if (linkRegion.Destination != destination)
                    {
                        linkRegion.Destination = destination;
                        if (_verbose)
                        {
                            Console.WriteLine("  Updated link");
                        }
                    }
                }
                else
                {
                    if (_verbose)
                    {
                        Console.WriteLine($"  Could not find heading to match label {linkRegion.Label}");
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
