using Mdfmt.Model;
using System;
using System.Linq;

namespace Mdfmt.Updaters;

/// <summary>
/// Add or remove line numbers in fenced code blocks as necessary according to 
/// a line numbering threshold.
/// </summary>
internal static class LineNumberUpdater
{
    public static void Update(MdStruct md, int lineNumberingThreshold, bool verbose)
    {
        bool removedLineNumbers = false;
        bool addedLineNumbers = false;

        foreach (FencedRegion fencedRegion in md.FencedRegions)
        {
            if (lineNumberingThreshold == 0)
            {
                removedLineNumbers = fencedRegion.RemoveLineNumbers() || removedLineNumbers;
                continue;
            }

            if (fencedRegion.ContentRegions.Count() >= lineNumberingThreshold)
            {
                addedLineNumbers = fencedRegion.AddLineNumbers() || addedLineNumbers;
            }
            else
            {
                removedLineNumbers = fencedRegion.RemoveLineNumbers() || removedLineNumbers;
            }
        }

        if (verbose)
        {
            if (removedLineNumbers)
            {
                Output.Emphasis("Removed line numbers from fenced code block(s)");
            }
            if (addedLineNumbers)
            {
                Output.Emphasis("Added line numbers to fenced code block(s)");
            }
        }
    }
}
