using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mdfmt;

/// <summary>
/// Perform needed updates to the Regions data structure, into which the Markdown file has been loaded.
/// </summary>
/// <param name="tocGenerator">Table of contents generator</param>
/// <param name="minimumEntryCount">The minimum number of entries in a Table of Contents before it will be added to a file</param>
/// <param name="linkDestinationGenerator">Generator for link destinations, to update links in the document</param>
/// <param name="verbose">Whether to generate extra output about what the program is doing</param>
internal class Updater(TocGenerator tocGenerator, int minimumEntryCount, ILinkDestinationGenerator linkDestinationGenerator, bool verbose)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;
    private readonly int _minimumEntryCount = minimumEntryCount;
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;
    private readonly bool _verbose = verbose;

    /// <summary>
    /// Sequence of content regions, with the content of the loaded Markdown file.  This is
    /// initialized on each call to Update().
    /// </summary>
    private Regions _regions;

    /// <summary>
    /// Headings of the Markdown file being updated.  This is initialized on each call to Update().
    /// </summary>
    private Headings _headings;

    /// <summary>
    /// Regular expression for parsing non-image links from Markdown.
    /// </summary>
    private static readonly Regex MarkdownLinkRegex = new(@"(?<!\!)\[(?<label>.*?)\]\((?<target>[^#\)]*)(?:#(?<fragment>[^)]*))?\)", RegexOptions.Compiled);
    /****
     * Deconstructing the MarkdownLinkRegex, for matching a Markdown link.
     * 
     * EXAMPLE OF A MARKDOWN LINK
     * 
     *   [2. Demo Overview](./Some-Demo.md#2.-demo-overview)
     * 
     * ANATOMY OF A MARKDOWN LINK
     * 
     *   [label](destination)
     *   
     * The destination has two sub-parts:
     * 1. Optional "target".  When present, the target is a relative path or it can be a URL.
     * 2. Optional "fragment".  When present the fragment immediately follows a '#' character
     *    and designates heading characters in some sort of a mangled or "slugified" form.
     *    
     * The destination part is typically typically just a target, just a fragment, or both.
     * It would be weird to have a completely empty destination; the Markdown linter certainly
     * considers an empty destination to be an error.  Note that when there's just a fragment,
     * that's how you often point to a heading in the same document.
     * 
     * PARTS OF Regex TO PARSE MARKDOWN LINK
     * 
     * 1. (?<!\!)
     * This is a lookbehind, to avoid matching a link that follows ! to avoid image links.
     * 
     * 2. \[(?<label>.*?)\]
     * This matches the label portion of a Markdown link, as follows:  Match an open square
     * bracket, capture a group with the key "label", and a closing square bracket.  The
     * group is captured non-greedily, meaning it is looking for the shortest possible match
     * to satisfy the expression.
     * 
     * 3. \(
     * Match open parenthesis of Markdown link destination.
     * 
     * 4. 
     * (?<target>[^#\)]*)
     * This captures a group with the key "target".  It uses a greedy match to capture characters
     * as long as they are not '#' (which signifies transition to link fragment) or ')' (which
     * is the end of the entire Markdown link).  While we are using a greedy match here, the 
     * negative character class [^#\)] keeps us safe and keeps us from parsing too far.
     * 
     * 5. (?:
     * Open a non-capturing group, so later, we can mark this whole group as optional.
     * 
     * 6. #
     * Match the '#' character, which separates the target from the fragment wihin the Markdown
     * link destination.
     * 
     * 7. (?<fragment>[^)]*)
     * This captures a group with the key "fragment".  It uses a greedy match to capture characters
     * as long as they are not the ')' that is the end of the entire Markdown link.  While we are 
     * using a greedy match here, the ')' negative character class [^)] keeps us safe so we won't
     * parse off the end of the link.
     * 
     * 8. )
     * Close the non-capturing group
     * 
     * 9. ?
     * Makes the non-capturing group optional.  This lets the destination of a Markdown link have 
     * a target without a fragement.
     * 
     * 10. \)
     * Match the closing parenthesis of Markdown link destination.  This ends the Markdown link.
     * 
     * PUTTING IT ALL TOGETHER
     * 
     * Assembling the pieces above you get:
     * (?<!\!)\[(?<label>.*?)\]\((?<target>[^#\)]*)(?:#(?<fragment>[^)]*))?\)
     ****/

    public void Update(Regions regions, Headings headings)
    {
        _regions = regions;
        _headings = headings;
        UpdateToc();
        UpdateLinks();
    }

    private void UpdateToc()
    {
        // Get List<Heading> headings, the headings in the document.
        if (!_headings.TryGetValue(_regions.Cpath, out List<Heading> headings))
        {
            headings = [];
        }

        // Generate a TOC based on headings.
        Toc newToc = _tocGenerator.GenerateToc(_regions.FileName, headings, _regions.NewlineSequence);

        // If the TOC is too small, get out.
        if (newToc.EntryCount < _minimumEntryCount)
        {
            return;
        }

        if (_regions.HasToc)
        {
            // There is a TOC.  Update it if necessary.
            Region tocRegion = _regions.FirstToc;
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
            if (_regions.HasFirstHeading)
            {
                _regions.InsertTocAfterFirstHeading(newToc.Content);
                if (_verbose)
                {
                    Console.WriteLine("  Inserted new TOC");
                }
            }
        }
    }

    private void UpdateLinks()
    {
        foreach (Region region in _regions)
        {
            if (region is AtomicRegion ar)
            {
                UpdateLinksIn(ar);
            }
            else
            {
                CompositeRegion cr = (CompositeRegion)region;
                foreach (AtomicRegion ar2 in cr)
                {
                    UpdateLinksIn(ar2);
                }
            }
        }
    }

    private void UpdateLinksIn(AtomicRegion ar)
    {
        if (ar.IsMutable && !ar.IsHeading && !ar.IsToc && ar.Content.Length > 0)
        {
            // Mutable non-heading, non-toc, with non-empty content.
            // This could have links!
            foreach (Match match in MarkdownLinkRegex.Matches(ar.Content))
            {
                if (LinkWithinSameFile(match.Groups["target"].Value))
                {
                    string label = match.Groups["label"].Value;
                    string link = match.Groups[0].Value;

                    if (_headings.TryGetValue(_regions.Cpath, label, out Heading heading))
                    {
                        string destination = _linkDestinationGenerator.GenerateLinkDestination(_regions.FileName, heading.Text);
                        string newLink = $"[{label}]({destination})";
                        string newContent = ar.Content.Replace(link, newLink);
                        if (ar.Content != newContent)
                        {
                            ar.Content = newContent;
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
                            Console.WriteLine($"  Could not find heading to match label {label}");
                        }
                    }
                }
            }
        }
    }

    private bool LinkWithinSameFile(string target)
    {
        return target == string.Empty || target.StartsWith($"./{_regions.FileName}");
    }

    //NOTE: Punting on relative links between files for now.  Maybe come back to it.
    // -SF 11/5/2024
    /*
    private bool LinkToOtherFileUsingRelativePath(string target)
    {
        return target.StartsWith($"./") || target.StartsWith($"../");
    }
    */
}
