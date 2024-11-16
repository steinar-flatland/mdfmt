namespace Mdfmt.Model;

/// <summary>
/// Abstract class for a region of content that is not subdivided into more regions.
/// There are two subclasses:
/// <see cref="MutableRegion"/> and
/// <see cref="ImmutableRegion"/>.
/// </summary>
public abstract class AtomicRegion2 : Region
{
    /// <inheritdoc/>
    public override string ActiveContent => IsActive ? Content : string.Empty;
}
