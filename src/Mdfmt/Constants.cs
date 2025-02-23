using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mdfmt;

internal static class Constants
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
    /// Set of string containing all the supported newlines.
    /// </summary>
    public static readonly IReadOnlySet<string> AllNewlines = new HashSet<string>() { UnixNewline, WindowsNewline };

    /// <summary>
    /// Wildcard expression for matching all Markdown files in the file system.
    /// </summary>
    public const string MdWildcard = "*.md";

    /// <summary>
    /// File extension indicating a Markdown file.
    /// </summary>
    public const string MdExtension = ".md";

    /// <summary>
    /// The maximum number of '#' symbols that can occur at the front of a Markdown heading.  If
    /// there are more than this many, then the content is regarded as just plain content region
    /// and not as a heading.
    /// </summary>
    public const int MaximumHeadingNumberSignCount = 6;

    /// <summary>
    /// Options commonly used for JSON serialization.
    /// </summary>
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

}
