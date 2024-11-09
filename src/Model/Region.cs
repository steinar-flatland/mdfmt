namespace Mdfmt.Model;

/// <summary>
/// Abstract class that holds a region of content from a Markdown file.  See also: 
/// <see cref="AtomicRegion"/>,
/// <see cref="CompositeRegion"/>.
/// </summary>
internal abstract class Region(bool isHeading, bool isToc)
{
    /// <summary>
    /// Whether this region has the FULL content of a Markdown heading.  (Note that an <c>AtomicRegion</c>
    /// with only part of the content of a heading has <c>IsHeading==false</c>, and the
    /// <c>CompositeRegion</c> that such an AtomicRegion is a part of has <c>IsHeading==true</c>.)
    /// </summary>
    public bool IsHeading { get; } = isHeading;

    /// <summary>
    /// Whether this region has a full table of contents.
    /// </summary>
    public bool IsToc { get; } = isToc;

    /// <summary>
    /// A string of content that is part of a Markdown file.
    /// </summary>
    public abstract string Content { get; set; }

    /// <summary>
    /// Whether the content may be mutated by Mdfmt.
    /// </summary>
    public abstract bool IsMutable { get; }

    /// <summary>
    /// The mutable content of this region.  Any immutable content is left out, as if it is
    /// commented out.  If this region contains no mutable content, returns empty string.
    /// </summary>
    //NOTE: In practice, immutable content might actually be in an HTML comment in the Markdown file.
    // It could also be from some other place that Mdfmt never touches, such as a fenced or indented
    // code region.
    public abstract string MutableContent { get; }

    /// <summary>
    /// Whether this region has been modified.
    /// </summary>
    public abstract bool IsModified { get; set; }
}
