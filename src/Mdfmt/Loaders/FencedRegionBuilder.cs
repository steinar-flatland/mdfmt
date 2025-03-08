using Mdfmt.Model;
using System;
using System.Collections.Generic;

namespace Mdfmt.Loaders;

/// <summary>
/// Builder class to help build a <see cref="FencedRegion"/>, in a way that is convenient for
/// implementing the <see cref="MdStructLoader"/>.
/// </summary>
internal class FencedRegionBuilder
{
    /// <summary>
    /// Regions to be incorporated into the <see cref="FencedRegion"/> being built.
    /// </summary>
    private readonly List<AtomicRegion> _regions = [];

    /// <summary>
    /// Whether the previous region added to the builder was a <see cref="NewlineRegion"/>.
    /// </summary>
    private bool _previousRegionWasNewline = false;

    /// <summary>
    /// Return the builder to a newly constructed state, with no regions, so it can be reused.
    /// </summary>
    public void Clear()
    {
        _regions.Clear();
        _previousRegionWasNewline = false;
    }

    /// <summary>
    /// Whether there are any regions in the builder.
    /// </summary>
    public bool HasRegions => _regions.Count > 0;

    /// <summary>
    /// Add an atomic region to the builder.  Throws <see cref="InvalidOperationException"/> if
    /// the <c>region</c> passed in is not of an expected subclass type that makes sense for
    /// building a <see cref="FencedRegion"/>.
    /// </summary>
    /// <param name="region">
    /// The region to add, which must be a <see cref="FenceMarkerRegion"/>, <see cref="ContentRegion"/>,
    /// or <see cref="NewlineRegion"/>.
    /// </param>
    /// <exception cref="InvalidOperationException"/>
    public void AddRegion(AtomicRegion region)
    {
        if (region is FenceMarkerRegion || region is ContentRegion)
        {
            _previousRegionWasNewline = false;
        }
        else if (region is NewlineRegion)
        {
            if (_previousRegionWasNewline)
            {
                // Add an empty region between newlines.  This makes it much easier to turn on line
                // numbering in a fenced region, if that is needed.
                _regions.Add(new ContentRegion(string.Empty));
            }
            _previousRegionWasNewline = true;
        }
        else
        {
            throw new InvalidOperationException($"Invalid type of region: {region.GetType().Name}");
        }
        _regions.Add(region);
    }

    /// <summary>
    /// Return a new <see cref="FencedRegion"/> containing the regions that were added to 
    /// the builder, and clear the builder to set it up for reuse.
    /// </summary>
    public FencedRegion BuildFencedRegion()
    {
        List<AtomicRegion> regionsShallowCopy = new(_regions);
        Clear(); // now that we have the shallow copy
        FencedRegion fencedRegion = new(regionsShallowCopy);
        return fencedRegion;
    }
}
