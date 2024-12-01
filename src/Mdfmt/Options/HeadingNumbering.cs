using System.Collections.Generic;

namespace Mdfmt.Options;

internal static class HeadingNumbering
{
    /// <summary>
    /// String indicating that no heading numbers are desired.  Evaluate in a case-insensitive way.
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// String indicating the desire for heading numbers that include a trailing period.
    /// </summary>
    public const string WithTrailingPeriod = "1.";

    /// <summary>
    /// String indicating the desire for heading numbers that do not include a trailing period.
    /// </summary>
    public const string WithoutTrailingPeriod = "1";

    /// <summary>
    /// This set defines the valid heading numbering options.
    /// </summary>
    public static readonly IReadOnlySet<string> Options = new HashSet<string>() { None, WithTrailingPeriod, WithoutTrailingPeriod };
}
