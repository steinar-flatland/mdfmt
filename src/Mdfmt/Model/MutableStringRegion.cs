namespace Mdfmt.Model;

/// <summary>
/// An atomic region with a mutable string of content.
/// Subclasses are:
/// <see cref="TocRegion"/>,
/// <see cref="FencedRegion"/>,
/// <see cref="IndentedLineRegion"/>,
/// <see cref="CommentRegion"/>,
/// <see cref="ContentRegion"/>
/// <see cref="LinkRegion"/>,
/// </summary>
public abstract class MutableStringRegion(string content) : MutableRegion
{
    /// <summary>
    /// String of content of this region.  It might be reset to a different string at some point.
    /// Backing storage for <c>Content</c> property.
    /// </summary>
    private string _content = content;

    /// <inheritdoc/>
    public override string Content
    {
        get => _content;
        set => SetContent(value);
    }

    protected void SetContent(string value)
    {
        // Once a modified state has occurred, there's no way to go back to unmodified.  If
        // not modified yet, then transition to a modified state will occur only when some
        // content is assigned, that is different from the existing content.  This prevents
        // entering a modified state for a frivolous reason.
        IsModified = IsModified || _content != value;
        _content = value;
    }
}
