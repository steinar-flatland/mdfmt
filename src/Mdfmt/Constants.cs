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
}
