namespace Mdfmt.Options;

public static class HeadingNumbers
{
    /// <summary>
    /// String indicating that no heading numbers are desired.  Evaluate in a case-insensitive way.
    /// </summary>
    public const string None = "None";

    /// <summary>
    /// String indicating the desire for heading numbers that include a trailing period.
    /// </summary>
    public const string WithTrailingPeriod = "1.";

    /// <summary>
    /// String indicating the desire for heading numbers that do not include a trailing period.
    /// </summary>
    public const string WithoutTrailingPeriod = "1";
}
