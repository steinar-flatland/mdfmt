using Mdfmt.Model;
using System;

namespace Mdfmt.Updaters;

/*
 * I need reusable logic to visit each line of a fenced code region.
 * Then I could do things like:
 * - Print the line
 * - Count the lines
 * - Evaluate the lines to see if each one starts with a line number
 * - Edit the lines
 * - etc.
 * 
 * I dismissed the idea of modelling a fenced code region as a composite region.
 * It seems awkward.  e.g. Imagine a fenced code block that has start marker,
 * newline, end marker.  Inside that region is 2 lines.  To add numbers,
 * you'd need to add 2 content regions.  And then you'd have to delete
 * those regions later.  Its just awkward.
 * 
 * A LinkRegion has quite a bit of structure that gets set up on cons.
 * Could we use a strategy like this?
 * Could we somehow build more strucure into the FencedRegion?
 * 
 * Right now, the content of a FencedRegion is just a string; its a little document.
 * You could parse it with the FileContentParser, similar to how MdStructLoader does.
 * 
 * But... should I try to structure it?
 * It would be so nice to have a structure that makes it easy to evaluate
 * whether there are numbers and turn numbers on and off...
 * Its just a bit tricky to build that.
 * 
 * This is harder than I thought :-(
 * 
 * 
 */

internal static class LineNumberUpdater
{
    public static void Update(MdStruct md, int lineNumberingThreshold, bool verbose)
    {
        Console.WriteLine($"lineNumberingThreshold = {lineNumberingThreshold}");
        Console.WriteLine("Printing out fenced regions:");

        int i = 0;
        foreach (FencedRegion fencedRegion in md.FencedRegions)
        {
            Console.WriteLine($"FENCED REGION {++i}");
            Console.WriteLine(fencedRegion.Content);
        }
    }
}
