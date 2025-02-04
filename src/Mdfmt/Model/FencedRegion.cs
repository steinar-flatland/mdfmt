using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mdfmt.Model;

/// <summary>
/// Contains a fenced code block from a Markdown document.  It is a composite region that always starts
/// with a <see cref="FenceMarkerRegion"/>, followed by <see cref="NewlineRegion"/>s and
/// <see cref="ContentRegion"/>s, and usually (but not necessarily) ending with a closing
/// <see cref="FenceMarkerRegion"/>.  The missing closing fence marker could occur if the Markdown file
/// ends in the middle of a fenced code block that does not have a closing fence marker.  Runs of adjacent
/// newlines are always separated by empty content regions.  This makes it easier to turn line numbers
/// on and off.
/// </summary>
/// <param name="atomicRegions">Atomic regions of this fenced region</param>
internal class FencedRegion(IReadOnlyList<AtomicRegion> atomicRegions) : CompositeRegion(atomicRegions)
{
    /// <summary>
    /// Regex for determining whether a line starts with what looks like a line number:
    /// <list type="bullet">
    ///   <item><c>^</c> matches the beginning of the string.</item>
    ///   <item><c>\s*</c> matches zero or more whitespace characters.</item>
    ///   <item><c>(\d+)</c> captures one or more digits into capture group 1.</item>
    ///   <item><c>(?: |$)</c> a non-capturing group that matches either a literal space () or the end of line.</item>
    /// </list>
    /// </summary>
    private static readonly Regex _lineNumberRegex = new(@"^\s*(\d+)(?: |$)", RegexOptions.Compiled);

    public bool HasLineNumbers => DetermineWhetherHasLineNumbers();

    /// <summary>
    /// Enumerable over the content regions of this composite regions.
    /// </summary>
    public IEnumerable<ContentRegion> ContentRegions => _atomicRegions.OfType<ContentRegion>();

    /// <summary>
    /// Determine whether this fenced code block has line numbers.
    /// </summary>
    /// <returns>bool</returns>
    private bool DetermineWhetherHasLineNumbers()
    {
        int lineNumber = 0;
        foreach (ContentRegion contentRegion in ContentRegions)
        {
            Match match = _lineNumberRegex.Match(contentRegion.Content);
            if ( (!match.Success) ||
                 (!int.TryParse(match.Groups[1].Value, out int number)) ||
                 (number != ++lineNumber) )
                return false;
        }
        return true;
    }

    /// <summary>
    /// Idempotently ensure that the content of this fenced region has line numbers.
    /// </summary>
    /// <returns>
    /// Whether it is the case that a transition from not having line numbers, to having line
    /// numbers, occurred.
    /// </returns>
    public bool AddLineNumbers()
    {
        if (HasLineNumbers) return false;
        int lineCount = ContentRegions.Count();
        int fieldWidth = lineCount.ToString().Length;
        int lineNumber = 0;
        foreach (ContentRegion region in ContentRegions)
        {
            string lineNumberContent = (++lineNumber).ToString().PadLeft(fieldWidth, ' ');
            if (region.Content.Length == 0)
            {
                region.Content = lineNumberContent;
            }
            else
            {
                region.Content = $"{lineNumberContent} {region.Content}";
            }
        }
        return true;
    }

    /// <summary>
    /// Idempotently ensure that the content of this fenced region does not have line numbers.
    /// </summary>
    /// <returns>
    /// Whether it is the case that a transition from having line numbers, to not having line
    /// numbers, occurred.
    /// </returns>
    public bool RemoveLineNumbers()
    {
        if (!HasLineNumbers) return false;
        foreach (ContentRegion region in ContentRegions)
        {
            region.Content = _lineNumberRegex.Replace(region.Content, "");
        }
        return true;
    }

}
