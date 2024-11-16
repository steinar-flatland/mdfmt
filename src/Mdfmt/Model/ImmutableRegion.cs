using System;

namespace Mdfmt.Model;

/// <summary>
/// An atomic region that is immutable.  See: <see cref="NewlineRegion"/>.
/// </summary>
public abstract class ImmutableRegion : AtomicRegion2
{
    /// <inheritdoc/>
    public override bool IsModified
    {
        get => false;
        protected set
        {
            throw new InvalidOperationException($"Class {GetType().Name} does not support modification");
        }
    }

    public override string Content
    {
        get
        {
            return string.Empty; // subclasses override
        }
        set
        {
            throw new InvalidOperationException($"Class {GetType().Name} does not support modification");
        }
    }
}
