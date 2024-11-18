using System;
using System.Collections.Generic;
using System.Linq;

namespace Mdfmt.Model;

public static class RegionIEnumerableExtensions
{
    public static string Content(this IEnumerable<Region> list)
    {
        return string.Concat(list.Select(r => r.Content));
    }

    public static bool IsActive(this IEnumerable<Region> list)
    {
        return list.Any(r => r.IsActive);
    }

    public static string ActiveContent(this IEnumerable<Region> list)
    {
        string result = string.Concat(list.Select(r => r.ActiveContent));
        return result;
    }

    public static bool IsModified(this IEnumerable<Region> list)
    {
        return list.Any(r => r.IsModified);
    }

    public static IEnumerable<Type> Types(this IEnumerable<Region> list)
    {
        return list.Select(r => r.GetType());
    }

    public static bool IsHeading(this IEnumerable<Region> list)
    {
        // If the active content, when trimmed, starts with from 1 to 6 '#' characters, then it is a heading.
        string headingText = string.Concat(list.Select(r => r.ActiveContent)).TrimStart();
        int i = 0;
        int numberSignCount = 0;
        while (i < headingText.Length && headingText[i] == '#')
        {
            numberSignCount++;
            i++;
        }
        return numberSignCount > 0  && numberSignCount <= Constants.MaximumHeadingNumberSignCount;
    }
}
