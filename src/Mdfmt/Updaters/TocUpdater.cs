﻿using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using System;

namespace Mdfmt.Updaters;

public class TocUpdater(TocGenerator tocGenerator)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;

    public void Update(MdStruct md, int minimumEntryCount, bool verbose)
    {
        // Generate a TOC based on headings.
        Toc newToc = _tocGenerator.GenerateToc(md.FileName, md.HeadingRegions, md.Newline);

        // Is the desired end state to have a TOC with the content of newToc in the document?
        bool tocShouldExist =
            minimumEntryCount > 0 &&
            newToc.EntryCount >= minimumEntryCount;

        if (md.HasToc) // The document has a TOC already.
        {
            if (tocShouldExist)
            {
                Region tocRegion = md.TocRegion;
                if (tocRegion.Content != newToc.Content)
                {
                    tocRegion.Content = newToc.Content;
                    if (verbose)
                    {
                        Console.WriteLine("  Updated TOC");
                    }
                }
            }
            else
            {
                md.DeleteToc();
                Console.WriteLine("  Removed TOC");
            }
        }
        else // The document does not have a TOC.
        {
            if (tocShouldExist)
            {
                md.AddToc(newToc.Content);
                if (verbose)
                {
                    Console.WriteLine("  Inserted new TOC");
                }
            }
        }
    }
}
