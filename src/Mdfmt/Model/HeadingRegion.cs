using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mdfmt.Model;

internal class HeadingRegion(IReadOnlyList<AtomicRegion> atomicRegions) : CompositeRegion(atomicRegions)
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
        return (ContentRegion)_atomicRegions.First(r => r is ContentRegion);
    }

    /// <summary>
    /// Update heading number to the new heading number passed in.
    /// </summary>
    /// <param name="newHeadingNumber">New heading number or null or empty for no heading number</param>
    /// <returns>Whether the heading number changed</returns>
    public bool SetHeadingNumber(string newHeadingNumber)
    {
        ContentRegion contentRegion = FirstContentRegion();
        string content = contentRegion.Content;
        Match match = HeadingNumberRegex.Match(content);

        if (match.Success)
        {
            string headingStart = match.Groups[HeadingStartGroup].Value;
            string headingNumber = match.Groups[HeadingNumberGroup].Value;
            string rest = match.Groups[RestGroup].Value;

            if (string.IsNullOrEmpty(newHeadingNumber))
            {
                contentRegion.Content = $"{headingStart}{rest}";
                return headingNumber.Length > 0;
            }
            else
            {
                newHeadingNumber = newHeadingNumber.Trim();
                contentRegion.Content = $"{headingStart}{newHeadingNumber} {rest}";
                headingNumber = headingNumber.Trim();
                return (headingNumber != newHeadingNumber);
            }
        }

        // Heading number was not modified.
        return false;
    }

}