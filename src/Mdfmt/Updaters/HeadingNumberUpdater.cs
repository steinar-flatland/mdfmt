using Mdfmt.Model;
using Mdfmt.Options;
using System.Text;
using System;

namespace Mdfmt.Updaters;

internal static class HeadingNumberUpdater
{
    public static void Update(MdStruct md, string headingNumbering, bool verbose)
    {
        bool headingsModified = false;

        if (headingNumbering.Equals(HeadingNumbering.None, StringComparison.OrdinalIgnoreCase))
        {
            foreach (HeadingRegion headingRegion in md.HeadingRegions)
            {
                headingsModified = headingRegion.SetHeadingNumber("") || headingsModified;
            }
        }
        else if (
            headingNumbering == HeadingNumbering.WithTrailingPeriod ||
            headingNumbering == HeadingNumbering.WithoutTrailingPeriod)
        {
            // Buffer used to assign multi-level heading numbers like 1.2.3.
            int[] counters = new int[Constants.MaximumHeadingNumberSignCount];

            // String buffer used to make a heading number.
            StringBuilder sb = new();

            // Previous index into numbers, or -1 if none.
            int prevN = -1;

            foreach (HeadingRegion headingRegion in md.HeadingRegions)
            {
                // Heading level.  # is level 0, ## is level 1, ### is level 2, etc.
                int level = headingRegion.Level;

                // Index into numbers where heading number counter is.  Edge case: n is -1 for heading level 0,
                // indicating that we don't count at that level:  That's the document title.
                int n = headingRegion.Level - 1;

                // Beware of documents with too many heading levels.
                if (n == counters.Length)
                {
                    Output.Warn($"Only {counters.Length} levels of heading numbering are supported.");
                    // Short circuit the execution of this method, so other updaters can still run.  
                    // To the extent that this method DID change any headings before it ran into heading
                    // overflow, its still good to run other updaters so that they can patch up 
                    // links to headings that changed, update the TOC, etc.
                    break;
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
                if (headingNumbering == HeadingNumbering.WithTrailingPeriod)
                    sb.Append('.');
                headingsModified = headingRegion.SetHeadingNumber(sb.ToString()) || headingsModified;

                // Set up for next iteration.  This helps us know when to zero out counters.
                prevN = n;
            } // end foreach headingRegion
        } // end if

        if (headingsModified && verbose)
        {
            Output.Emphasis("Updated heading numbers");
        }

    } // end Update()
}