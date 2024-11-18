using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mdfmt.Model;

public class HeadingRegion(IReadOnlyList<AtomicRegion> atomicRegions) : CompositeRegion(atomicRegions)
{
    private static readonly Regex HeadingNumberRegex = new(@"^(?<headingStart>\s*#+ )(?:(?<headingNumber>\d+(?:\.\d+)*\.?) )?(?<rest>.*)", RegexOptions.Compiled);
    private const string HeadingStartGroup = "headingStart";
    private const string HeadingNumberGroup = "headingNumber";
    private const string RestGroup = "rest";

    /// <summary>
    /// The level of the heading, which is an integer one less than the number of '#' characters in
    /// the Markdown.
    /// </summary>
    public int Level => ActiveContent.TrimStart().TakeWhile(c => c == '#').Count() - 1;

    /// <summary>
    /// Heading text in rendered Markdown document
    /// </summary>
    public string HeadingText => ActiveContent.TrimStart().TrimStart('#').Trim();


    /// <summary>
    /// Get the first content region of the heading.
    /// </summary>
    /// <returns>The first content region of the heading.</returns>
    public ContentRegion FirstContentRegion()
    {
        return (ContentRegion) _atomicRegions.First(r => r is ContentRegion);
    }

    /// <summary>
    /// Idempotently set the heading number to the value specified.  If you want to remove a
    /// heading number, then set the value null or empty.
    /// </summary>
    public string HeadingNumber
    {
        set
        {
            UpdateHeadingNumber(value);
        }
    }

    private void UpdateHeadingNumber(string headingNumber)
    {
        ContentRegion contentRegion = FirstContentRegion();
        string content = contentRegion.Content;
        Match match = HeadingNumberRegex.Match(content);
        if (match.Success)
        {
            string headingStart = match.Groups[HeadingStartGroup].Value;
            string rest = match.Groups[RestGroup].Value;
            if (string.IsNullOrEmpty(headingNumber))
            {
                contentRegion.Content = $"{headingStart}{rest}";
            }
            else
            {
                contentRegion.Content = $"{headingStart}{headingNumber.Trim()} {rest}";
            }
        }
    }

}
