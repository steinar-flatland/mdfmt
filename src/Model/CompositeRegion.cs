using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mdfmt.Model;

/// <summary>
/// An composite region of Markdown content.  "Composite" means that this region is composed of
/// a collection of atomic regions.  Related class:  <see cref="AtomicRegion"/>.
/// </summary>
internal class CompositeRegion(IReadOnlyList<AtomicRegion> atomicRegions, bool isHeading, bool isToc = false) : Region(isHeading, isToc), IEnumerable<AtomicRegion>
{
    /// <summary>
    /// List of atomic regions that make up this composite region.  Backing storage for <c>Content</c> property.
    /// </summary>
    private readonly IReadOnlyList<AtomicRegion> _atomicRegions = atomicRegions;

    /// <inheritdoc/>
    public override string Content
    {
        get
        {
            return string.Concat(_atomicRegions.Select(ar => ar.Content));
        }
        set
        {
            throw new InvalidOperationException
                ($"Setting {nameof(Content)} of a {nameof(CompositeRegion)} is not supported. " +
                $"You may set the {nameof(Content)} in the underlying {nameof(AtomicRegion)}s.");
            //NOTE: We could implement a separate ReplaceContent region that installs a whole
            // new list of AtomicRegion, but there's no use case for that at the moment.
            // If that changes, implement that method and remove this note. 
        }
    }

    /// <inheritdoc/>
    public override bool IsMutable => _atomicRegions.Any(ar => ar.IsMutable);

    /// <inheritdoc/>
    public override string MutableContent => string.Concat(_atomicRegions.Where(ar => ar.IsMutable).Select(ar => ar.Content));

    /// <inheritdoc/>
    public override bool IsModified
    {
        get
        {
            return _atomicRegions.Any(ar => ar.IsModified);
        }
        set
        {
            throw new InvalidOperationException
                ($"A {nameof(CompositeRegion)} cannot be explicitly marked as modified. Manage the " +
                $"{nameof(IsModified)} flags of the underlying {nameof(AtomicRegion)}s, and the containing " +
                $"{nameof(CompositeRegion)}'s {nameof(IsModified)} flag derives from that.");
        }
    }

    public IEnumerator<AtomicRegion> GetEnumerator()
    {
        return _atomicRegions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
