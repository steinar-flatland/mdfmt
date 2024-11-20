using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using Mdfmt.Options;
using System;

namespace Mdfmt.Updaters;

public class TocUpdater(CommandLineOptions options, TocGenerator tocGenerator) : UpdaterBase(options)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;

    public override void Update(MdStruct md)
    {
        // Generate a TOC based on headings.
        Toc newToc = _tocGenerator.GenerateToc(md.FileName, md.HeadingRegions, md.Newline);

        // Is the desired end state to have a TOC with the content of newToc in the document?
        bool tocShouldExist =
            _options.MinimumEntryCount > 0 &&
            newToc.EntryCount >= _options.MinimumEntryCount;

        if (md.HasToc) // The document has a TOC already.
        {
            if (tocShouldExist)
            {
                Region tocRegion = md.TocRegion;
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
                md.DeleteToc();
                Console.WriteLine("  Removed TOC");
            }
        }
        else // The document does not have a TOC.
        {
            if (tocShouldExist)
            {
                md.AddToc(newToc.Content);
                if (_options.Verbose)
                {
                    Console.WriteLine("  Inserted new TOC");
                }
            }
        }
    }
}
