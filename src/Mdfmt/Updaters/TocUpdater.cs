using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using System;

namespace Mdfmt.Updaters;

/// <summary>
/// This class is used in the context of a positive TOC threshold, to determine whether a TOC
/// should exist and to add, update, or remove the TOC as necessary.
/// </summary>
/// <param name="tocGenerator">Used to generate a new version of the TOC based on the document's headings.</param>
internal class TocUpdater(TocGenerator tocGenerator)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;

    /// <summary>
    /// <para>
    /// Handle the case of adding, updating or removing the TOC when it has been asserted, through
    /// a positive TOC threshold, that there are at least some conditions under which a 
    /// TOC should exist.
    /// </para>
    /// <para>
    /// Note that the case of<c> tocThreshold == 0</c> is not covered by this class.  That case is
    /// covered by calling <see cref="TocDeleter"/> directly.
    /// </para>
    /// <para>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if a value less than 1 is passed to <c>tocThreshold</c>.
    /// </para>
    /// </summary>
    /// <param name="md">
    /// A Markdown file loaded into <see cref="MdStruct"/>
    /// </param>
    /// <param name="tocThreshold">
    /// A positive tocThreshold, indicating the minimum number of headings for which to include a TOC.
    /// </param>
    /// <param name="verbose">
    /// Whether to generate verbose output.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public void Update(MdStruct md, int tocThreshold, bool verbose)
    {
        if (tocThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(tocThreshold), tocThreshold, "Must be positive to utilize this class.");
        }

        bool tocShouldExist = md.HeadingCount >= tocThreshold;

        if (!tocShouldExist)
        {
            TocDeleter.DeleteToc(md, verbose);
            return;
        }

        // Generate a TOC based on headings.
        Toc newToc = _tocGenerator.GenerateToc(md.FileName, md.HeadingRegions, md.Newline);

        // Install the new TOC.  Details vary based on whether TOC already exists.

        if (md.HasToc) // The document has a TOC already.
        {
            TocRegion tocRegion = md.TocRegion;
            if (tocRegion.Content != newToc.Content)
            {
                tocRegion.Content = newToc.Content;
                if (verbose)
                {
                    Output.Emphasis("Updated TOC");
                }
            }
        }
        else // The document does not have a TOC.
        {
            md.AddToc(newToc.Content);
            if (verbose)
            {
                Output.Emphasis("Inserted new TOC");
            }
        }
    }
}
