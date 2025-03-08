using Mdfmt.Model;
using Mdfmt.Options;
using System.Text;
using System;
using System.Collections.Generic;

namespace Mdfmt.Updaters;

/// <summary>
/// Updates heading numbers in section headings.
/// </summary>
internal class HeadingNumberUpdater()
{
    /// <summary>
    /// Backing storage for <c>OldToNewHeadingMap</c>.
    /// </summary>
    private Dictionary<string, string> _oldToNewHeadingMap;

    /// <summary>
    /// Dictionary mapping old heading to new heading for each heading that changed on the latest
    /// call to <c>Update()</c>.  Does not include headings that are repeated in the document being
    /// processed before applying changes.
    /// </summary>
    public IReadOnlyDictionary<string, string> OldToNewHeadingMap => _oldToNewHeadingMap;

    /// <summary>
    /// Backing storage for <c>DuplicateHeadings</c>.
    /// </summary>
    private HashSet<string> _duplicateHeadings;

    /// <summary>
    /// Headings that are duplicated in the document being processed before applying changes
    /// on the latest call to <c>Update()</c>. These headings cannot be included in the
    /// <c>OldToNewHeadingMap</c>, because there would be a key collision.
    /// </summary>
    public IReadOnlySet<string> DuplicateHeadings => _duplicateHeadings;

    /// <summary>
    /// Reset the state of this object.
    /// </summary>
    private void Reset()
    {
        _oldToNewHeadingMap = [];
        _duplicateHeadings = [];
    }

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
    public void Update(MdStruct md, string headingNumbering, bool verbose)
    {
        Reset();

        // Whether any headings have been modified.
        bool headingsModified = false;

        if (headingNumbering.Equals(HeadingNumbering.None, StringComparison.OrdinalIgnoreCase))
        {
            foreach (HeadingRegion headingRegion in md.HeadingRegions)
            {
                headingsModified = SetHeadingNumber(headingRegion, string.Empty) || headingsModified;
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

                headingsModified = SetHeadingNumber(headingRegion, sb.ToString()) || headingsModified;

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
            Output.Emphasis("- Updated heading numbers");
        }

    } // end Update()

    /// <summary>
    /// Update the heading number of a heading region and maintain <c>_oldToNewHeadingMap</c> and
    /// <c>_duplicateHeadings</c>.
    /// </summary>
    /// <param name="headingRegion">
    /// The <see cref="HeadingRegion"/> to update.
    /// </param>
    /// <param name="headingNumber">
    /// New value for heading number e.g. <c>"1."</c> or empty string if none.
    /// </param>
    /// <returns>
    /// Whether the heading region was modified.
    /// </returns>
    private bool SetHeadingNumber(HeadingRegion headingRegion, string headingNumber)
    {
        string oldHeading = headingRegion.HeadingText;
        bool modified = headingRegion.SetHeadingNumber(headingNumber);
        if (modified && !_duplicateHeadings.Contains(oldHeading))
        {
            string newHeading = headingRegion.HeadingText;
            if (_oldToNewHeadingMap.Remove(oldHeading))
            {
                _duplicateHeadings.Add(oldHeading);
            }
            else
            {
                _oldToNewHeadingMap[oldHeading] = newHeading;
            }
        }
        return modified;
    }

}
