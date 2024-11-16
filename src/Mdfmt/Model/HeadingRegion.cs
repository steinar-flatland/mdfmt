using System;
using System.Collections.Generic;
using System.Linq;

namespace Mdfmt.Model;

public class HeadingRegion(IReadOnlyList<AtomicRegion2> atomicRegions) : CompositeRegion2(atomicRegions)
{
    /// <summary>
    /// The level of the heading, which is an integer one less than the number of '#' characters in
    /// the Markdown.
    /// </summary>
    public int Level => ActiveContent.TrimStart().TakeWhile(c => c == '#').Count() - 1;

    /// <summary>
    /// Heading text in rendered Markdown document
    /// </summary>
    public string HeadingText => ActiveContent.TrimStart().TrimStart('#').Trim();
}
