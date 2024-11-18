using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mdfmt.Model;

/// <summary>
/// Abstract class for a region of content that is composed of a sequence of subregions.  Currently
/// there is only one concrete implementation: <see cref="HeadingRegion"/>.
/// </summary>
public abstract class CompositeRegion(IReadOnlyList<AtomicRegion> atomicRegions) : Region, IEnumerable<AtomicRegion>
{
    /// <summary>
    /// List of atomic regions that make up this composite region.  Backing storage for <c>Content</c> property.
    /// </summary>
    protected readonly IReadOnlyList<AtomicRegion> _atomicRegions = atomicRegions;

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
                ($"Set {nameof(Content)} of class {GetType().Name} is not supported. " +
                $"You may set the {nameof(Content)} in the underlying {nameof(AtomicRegion)}s.");
        }
    }

    /// <inheritdoc/>
    public override bool IsActive => _atomicRegions.Any(ar => ar.IsActive);

    /// <inheritdoc/>
    public override string ActiveContent => string.Concat(_atomicRegions.Select(ar => ar.ActiveContent));

    /// <inheritdoc/>
    public override bool IsModified
    {
        get => _atomicRegions.Any(ar => ar.IsModified);
        protected set
        {
            throw new InvalidOperationException($"Set {nameof(IsModified)} of class {GetType().Name} is not supported.");
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
