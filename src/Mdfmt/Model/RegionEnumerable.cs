using System.Collections;
using System.Collections.Generic;

namespace Mdfmt.Model;

/// <summary>
/// Provides an <c>IEnumerable</c> for enumerating the subset of regions, from a larger
/// sequence of <c>Region</c>, that are of a specified type <c>T</c>.
/// </summary>
/// <typeparam name="T">The type of <c>Region</c> to enumerate.</typeparam>
internal class RegionEnumerable<T>(IEnumerable<Region> regions) : IEnumerable<T>
{
    private readonly IEnumerable<Region> _regions = regions;

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var region in _regions)
        {
            if (region is T typedRegion)
            {
                yield return typedRegion;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
