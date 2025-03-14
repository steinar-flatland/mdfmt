﻿namespace Mdfmt.Model;

/// <summary>
/// Abstract class that holds a region of content from a Markdown file.  See also: 
/// <see cref="AtomicRegion"/>,
/// <see cref="CompositeRegion"/>.
/// </summary>
internal abstract class Region
{
    /// <summary>
    /// A string of content that is part of a Markdown file.  This can be empty but not null.
    /// The file that was loaded can be reassembled by concatenating all the content strings.
    /// </summary>
    public abstract string Content { get; set; }

    /// <summary>
    /// Whether any of the content of this region is active.
    /// </summary>
    public virtual bool IsActive { get; } = true;

    /// <summary>
    /// The active content of this region.  Active means not commented out.  If there is no active
    /// content, returns empty string.
    /// </summary>
    public abstract string ActiveContent { get; }

    /// <summary>
    /// Whether this region has been modified.
    /// </summary>
    public abstract bool IsModified { get; protected set; }

    public override string ToString() => Content;

    /// <summary>
    /// Name of type of region.  Just helps with visibility in debugger.
    /// </summary>
    public string TypeName => GetType().Name;
}
