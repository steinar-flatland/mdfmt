using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using Mdfmt.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mdfmt;

/// <summary>
/// Perform needed updates to the Regions data structure, into which the Markdown file has been loaded.
/// </summary>
/// <param name="tocGenerator">Table of contents generator</param>
/// <param name="linkDestinationGenerator">Generator for link destinations, to update links in the document</param>
/// <param name="verbose">Whether to generate extra output about what the program is doing</param>
public class Updater(TocGenerator tocGenerator, ILinkDestinationGenerator linkDestinationGenerator, CommandLineOptions options)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;
    private readonly CommandLineOptions _options = options;

    private MdStruct _md;

    public void Update(MdStruct md)
    {
        _md = md;

        // Make heading region dictionary, keyed on heading text and link destination.
        Dictionary<string, HeadingRegion> headingRegionDictionary = _md.MakeHeadingRegionDictionary();

        UpdateHeadingNumbers(); //Note: This can change headings!

        // Update TOC with latest headings.
        UpdateToc();

        // Updated links in the body of the document.  Use the headingRegionDictionary with stale
        // link destination keys to find current headings from the stale links in the body.
        UpdateLinks(headingRegionDictionary);
    }

    private void UpdateHeadingNumbers()
    {
        if (_options.HeadingNumbering.Equals(HeadingNumbers.None, StringComparison.OrdinalIgnoreCase))
        {
            foreach (HeadingRegion headingRegion in _md.HeadingRegions)
            {
                headingRegion.HeadingNumber = "";
            }
        }
        else if (
            _options.HeadingNumbering == HeadingNumbers.WithTrailingPeriod ||
            _options.HeadingNumbering == HeadingNumbers.WithoutTrailingPeriod)
        {
            // Buffer used to assign multi-level heading numbers like 1.2.3.
            int[] counters = new int[Constants.MaximumHeadingNumberSignCount];

            // String buffer used to make a heading number.
            StringBuilder sb = new();

            // Previous index into numbers, or -1 if none.
            int prevN = -1;

            foreach (HeadingRegion headingRegion in _md.HeadingRegions)
            {
                // Heading level.  # is level 0, ## is level 1, ### is level 2, etc.
                int level = headingRegion.Level;

                // Index into numbers where heading number counter is.  Edge case: n is -1 for heading level 0,
                // indicating that we don't count at that level:  That's the document title.
                int n = headingRegion.Level - 1;

                // Beware of documents with too many heading levels.
                if (n == counters.Length)
                {
                    Console.WriteLine($"Only {counters.Length} levels of heading numbering are supported.");
                    // Short circuit the execution of this method, so other updaters can still run.  
                    // To the extent that this method DID change any headings before it ran into heading
                    // overflow, its still good to run other updaters so that they can patch up 
                    // links to headings that changed, update the TOC, etc.
                    return;
                }

                // Handle edge case.  We will not assign a section number to a heading with only
                // one '#', which is typically just the title of the document.
                if (n < 0)
                {
                    prevN = n;
                    continue;
                }

                // If the previous heading was the document title, ensure all the counters are 0.
                if (prevN == -1)
                {
                    Array.Fill(counters, 0);
                }
                else if (prevN > n)
                {
                    // Zero out counters for more indented sections that are fully processed.
                    Array.Fill(counters, 0, n + 1, prevN - n);
                }

                // Up the current counter.
                counters[n]++;

                // Generate and assign heading number
                sb.Clear();
                for (int i = 0; i <= n; i++)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append('.');
                    }
                    sb.Append($"{counters[i]}");
                }
                if (_options.HeadingNumbering == HeadingNumbers.WithTrailingPeriod)
                    sb.Append('.');
                headingRegion.HeadingNumber = sb.ToString();

                // Set up for next iteration.  This helps us know when to zero out counters.
                prevN = n;
            }
        }
        else
        {
            Console.WriteLine($"Invalid choice of heading numbering: {_options.HeadingNumbering}.  Try the --help option.");
            Environment.Exit(ExitCodes.MisuseOfCommand);
        }
    }

    private void UpdateToc()
    {
        // Generate a TOC based on headings.
        Toc newToc = _tocGenerator.GenerateToc(_md.FileName, _md.HeadingRegions, _md.Newline);

        // Is the desired end state to have a TOC with the content of newToc in the document?
        bool tocShouldExist =
            _options.MinimumEntryCount > 0 &&
            newToc.EntryCount >= _options.MinimumEntryCount;

        if (_md.HasToc) // The document has a TOC already.
        {
            if (tocShouldExist)
            {
                Region tocRegion = _md.TocRegion;
                if (tocRegion.Content != newToc.Content)
                {
                    tocRegion.Content = newToc.Content;
                    if (_options.Verbose)
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
                if (_options.Verbose)
                {
                    Console.WriteLine("  Inserted new TOC");
                }
            }
        }
    }

    /// <summary>
    /// Update link destinations of links that navigate within the document, to be current  for
    /// possible changes due to heading numbering.
    /// </summary>
    /// <param name="headingRegionDictionary">
    /// Heading region dictionary, keyed on original heading text and link destinations.  The link
    /// destination keys will be stale if sections were renumbered, and they will lead to the updated
    /// heading regions.  This is good:  It allows us to look up a stale link destination, get a
    /// current heading region, and use the current heading to update the stale link destination.
    /// </param>
    private void UpdateLinks(Dictionary<string, HeadingRegion> headingRegionDictionary)
    {
        foreach (LinkRegion linkRegion in _md.LinkRegions)
        {
            if (LinkWithinSameFile(linkRegion.Destination))
            {
                if (headingRegionDictionary.TryGetValue(linkRegion.Label, out HeadingRegion headingRegion) ||
                    headingRegionDictionary.TryGetValue(linkRegion.Destination, out headingRegion))
                {
                    string destination = _linkDestinationGenerator.GenerateLinkDestination(_md.FileName, headingRegion.HeadingText);
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

    private bool LinkWithinSameFile(string destination)
    {
        return string.IsNullOrEmpty(destination) || destination.StartsWith('#') || destination.StartsWith($"./{_md.FileName}");
    }
}
