using System;

namespace Mdfmt.Model;

/// <summary>
/// This class is used to represent a newline character or sequence.  See subclasses
/// <see cref="UnixNewlineRegion"/> and
/// <see cref="WindowsNewlineRegion"/>.
/// This base class doesn't contain anything except some static utilities for conveniently getting
/// a NewlineRegion.
/// </summary>
public abstract class NewlineRegion : ImmutableRegion
{
    /// <summary>
    /// Suppress constructor, except for this class and subclasses.
    /// </summary>
    protected NewlineRegion() { }

    /// <summary>
    /// Get a <c>NewlineRegion</c> containing the specified newline character or sequence.
    /// </summary>
    /// <param name="newline">
    /// String indicating the desired kind of <c>NewlineRegion</c>, either <c>"\n"</c> or
    /// <c>"\r\n"</c></param>.
    /// <returns>Either a <c>UnixNewlineRegion</c> or a <c>WindowsNewlineRegion</c>.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static NewlineRegion Containing(string newline)
    {
        return newline switch
        {
            "\n" => UnixNewlineRegion.Instance,
            "\r\n" => WindowsNewlineRegion.Instance,
            _ => throw new ArgumentException("Unsupported newline character or sequence.", nameof(newline))
        };
    }
}
