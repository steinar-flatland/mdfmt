namespace Mdfmt.Model;

/// <summary>
/// An atomic region that is mutable.  This abstract class just adds on the ability for anyone
/// to check an <c>IsModified</c> flag, and for this class hierarchy to set the flag as a 
/// detail, when the content is changed.  See subclass:  <see cref="MutableStringRegion"/>.
/// </summary>
public abstract class MutableRegion : AtomicRegion
{
    /// <inheritdoc/>
    public override bool IsModified { get; protected set; }
}
