using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mdfmt.Model;

internal class FencedRegion(IReadOnlyList<AtomicRegion> atomicRegions) : CompositeRegion(Adjust(atomicRegions))
{
    private static readonly Regex _lineNumberRegex = new(@"^\s*(\d+)", RegexOptions.Compiled);

    /// <summary>
    /// Adjust the list of <see cref="AtomicRegion"/> passed in such that there are never two consecutive
    /// <see cref="NewlineRegion"/>.  Whenever two newline regions are seen in a row, insert an empty
    /// <see cref="ContentRegion"/> between them.  This is done to make it easy to add line numbers
    /// to a <c>FencedRegion</c>:  Just visit all the <c>ContentRegion</c>s.
    /// </summary>
    /// <param name="atomicRegions"></param>
    /// <returns></returns>
    private static List<AtomicRegion> Adjust(IReadOnlyList<AtomicRegion> atomicRegions)
    {
        List<AtomicRegion> result = [];
        bool previousRegionWasNewlineRegion = false;
        foreach (AtomicRegion region in atomicRegions)
        {
            if (region is FenceMarkerRegion || region is ContentRegion)
            {
                previousRegionWasNewlineRegion = false;
            }
            else if (region is NewlineRegion)
            {
                if (previousRegionWasNewlineRegion)
                {
                    result.Add(new ContentRegion(""));
                }
                previousRegionWasNewlineRegion = true;
            }
            else
            {
                throw new InvalidOperationException($"Unhandled type of atomic region: {region.GetType().Name}");
            }
            result.Add(region);
        }
        return result;
    }

    public bool HasLineNumbers => DetermineWhetherHasLineNumbers();

    /// <summary>
    /// Enumerable over the content regions of this composite regions.
    /// </summary>
    public IEnumerable<AtomicRegion> ContentRegions => _atomicRegions.OfType<ContentRegion>();



    private bool DetermineWhetherHasLineNumbers()
    {
        // Enumerate the ContentRegions, and let n be 1-based index into the enumeration.
        // Return if for each region, its content matches the _lineNumberRegex and the capture group, converted to int, = n.
        // Otherwise return false

    }

    public void AddLineNumbers()
    {
        // Idempotently add line numbers to the fenced region
    }

    public void RemoveLineNumbers()
    {
        // Idempotently remove line numbers from the fenced region
    }

}
