namespace Mdfmt;

public static class Constants
{
    /// <summary>
    /// String containing Unix newline character.
    /// </summary>
    public const string UnixNewline = "\n";

    /// <summary>
    /// String containing Windows newline sequence.
    /// </summary>
    public const string WindowsNewline = "\r\n";

    /// <summary>
    /// Array of string containing all the supported newlines.
    /// </summary>
    // For correct splitting behavior, keep longer strings before shorter ones.
    public static readonly string[] AllNewlines = [WindowsNewline, UnixNewline];

    /// <summary>
    /// The maximum number of '#' symbols that can occur at the front of a Markdown heading.  If
    /// there are more than this many, then the content is regarded as just plain content region
    /// and not as a heading.
    /// </summary>
    public const int MaximumHeadingNumberSignCount = 6;
}
