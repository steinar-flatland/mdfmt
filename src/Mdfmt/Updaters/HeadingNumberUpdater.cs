using Mdfmt.Model;
using Mdfmt.Options;
using System.Text;
using System;

namespace Mdfmt.Updaters;

/// <summary>
/// Updates heading numbers in section headings.
/// </summary>
internal static class HeadingNumberUpdater
{
    /// <summary>
    /// Ensure that each heading in the Markdown document either does or does not have a heading
    /// number, as per a provided option value.
    /// </summary>
    /// <param name="md">
    /// A Markdown file loaded into <see cref="MdStruct"/>.
    /// </param>
    /// <param name="headingNumbering">
    /// One of the options for heading numbering as defined in <see cref="Options.HeadingNumbering"/>.
    /// </param>
    /// <param name="verbose">
    /// Whether to generate verbose output.
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
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
        }
        else
        {
            throw new InvalidOperationException($"Unhandled kind of heading numbering: {headingNumbering}");
        }

        if (headingsModified && verbose)
        {
            Output.Emphasis("Updated heading numbers");
        }

    } // end Update()
}