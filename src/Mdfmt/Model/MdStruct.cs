﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mdfmt.Model;

/// <summary>
/// A Markdown file that has been loaded into a data structure.
/// </summary>
/// <param name="filePath">
/// The Markdown file that was loaded.
/// </param>
/// <param name="regions">
/// Regions parsed by MdStructLoader.
/// </param>
/// <param name="isModified">
/// Whether this MdStruct should be already viewed as modified from the outset.  This may seem
/// strange, but it is possible in a scenario where the user is loading up the file to save
/// it back out with a differnt style of newline.
/// </param>
/// <param name="newlineRegion">
/// The newline region being used in this file.
/// </param>
public class MdStruct(
    string filePath,
    List<Region> regions,
    bool isModified,
    NewlineRegion newlineRegion
    )
{
    /// <summary>
    /// Path of Markdown file from which this data structure was loaded.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// Regions of the Markdown file.
    /// </summary>
    private readonly List<Region> _regions = regions;

    /// <summary>
    /// <para>
    /// Whether <c>MdStruct</c> has been modified (other than a change to one of the
    /// <c>Region</c> instances in <c>_regions</c>) in a way that requires saving the Markdown file
    /// back out to disk.  Separately, there is also an <c>IsModified</c> property on each
    /// <c>Region</c>.  This <c>_isModified</c> field, or'd with the <c>IsModified</c> properties
    /// of the <c>Region</c> objects of <c>_regions</c>, provide the state behind the <c>IsModified</c>
    /// property of <c>Mdfmt</c>.
    /// </para>
    /// <para>
    /// On initial constuction of <c>MdStruct</c>, <c>_isModified</c> could already be true, if the
    /// user intends to write the Markdown file out a different way, with different newline characters.
    /// </para>
    /// <para>
    /// This flag is used to notice changes that are not about editing an existing <c>Region</c>, such
    /// as inserting new regions.
    /// </para>
    /// </summary>
    private bool _isModified = isModified;

    /// <summary>
    /// Whether this data structure has been modified since it was loaded.
    /// </summary>
    public bool IsModified => _isModified || _regions.Any(r => r.IsModified);

    /// <summary>
    /// The newline region being used in this file.
    /// </summary>
    private readonly NewlineRegion _newlineRegion = newlineRegion;

    /// <summary>
    /// String containing the newline character or sequence being used for line termination.
    /// </summary>
    public string Newline => _newlineRegion.Content;

    /// <summary>
    /// Simple file name of the Markdown file, without any path information.
    /// </summary>
    public string FileName { get; } = Path.GetFileName(filePath);

    /// <summary>
    /// Expose enumerator of <c>Region</c> objects that hold the content of the loaded Markdown document.
    /// </summary>
    public IEnumerable<Region> Regions => _regions;

    /// <summary>
    /// The number of regions.
    /// </summary>
    public int RegionCount => _regions.Count;

    /// <summary>
    /// Enumerate the regions of type <c>LinkRegion</c>.
    /// </summary>
    public IEnumerable<LinkRegion> LinkRegions { get; } = new RegionEnumerable<LinkRegion>(regions);

    /// <summary>
    /// The number of link regions.
    /// </summary>
    public int LinkCount => LinkRegions.Count();

    /// <summary>
    /// Whether this Markdown document has one or more links.
    /// </summary>
    public bool HasLink => LinkRegions.Any();

    /// <summary>
    /// Enumerate the regions of type <c>HeadingRegion</c>.
    /// </summary>
    public IEnumerable<HeadingRegion> HeadingRegions { get; } = new RegionEnumerable<HeadingRegion>(regions);

    /// <summary>
    /// The number of heading regions.
    /// </summary>
    public int HeadingCount => HeadingRegions.Count();

    /// <summary>
    /// Whether this Markdown document has one or more headings.
    /// </summary>
    public bool HasHeading => HeadingRegions.Any();

    /// <summary>
    /// Enumerate the regions of type <c>TocRegion</c>.
    /// </summary>
    public IEnumerable<TocRegion> TocRegions { get; } = new RegionEnumerable<TocRegion>(regions);

    /// <summary>
    /// The table of contents of this document or null if none.  (If there is more than one
    /// TOC, returns the first one.)
    /// </summary>
    public TocRegion TocRegion => TocRegions.FirstOrDefault();

    /// <summary>
    /// The number of TOC regions.
    /// </summary>
    public int TocCount => TocRegions.Count();

    /// <summary>
    /// Whether this Markdown document has a table of contents.
    /// </summary>
    public bool HasToc => TocRegions.Any();

    /// <summary>
    /// Insert a table of contents after the first heading.  Add a blank line between the heading and the new TOC.
    /// An <c>InvalidOperationException</c> will occur if there is already a TOC, or if it is not the case that
    /// there is a first heading to insert after.
    /// </summary>
    /// <param name="tocContent">string with content for the TOC</param>
    /// <exception cref="InvalidOperationException"/>
    public void InsertTocAfterFirstHeading(string tocContent)
    {
        if (HasToc)
        {
            throw new InvalidOperationException($"{FilePath} already has a TOC.");
        }
        int firstHeadingIndex = _regions.FindIndex(r => r is HeadingRegion);
        if (firstHeadingIndex == -1)
        {
            throw new InvalidOperationException($"Cannot insert TOC into {FilePath} because there is no heading to insert after.");
        }
        TocRegion tocRegion = new(tocContent);
        _regions.Insert(firstHeadingIndex + 1, tocRegion);
        _regions.Insert(firstHeadingIndex + 1, _newlineRegion);
        _regions.Insert(firstHeadingIndex + 1, _newlineRegion);
        _isModified = true;
    }

    /// <summary>
    /// The content of this Markdown file, reassembled from the regions of the <c>MdStruct</c> data structure.
    /// </summary>
    public string Content => string.Concat(_regions.Select(r => r.Content));

    /// <summary>
    /// Dictionary for finding instances of <c>HeadingRegion</c>, keyed on heading text.  Heading
    /// text is the heading as displayed in a rendered Markdown document.  When a document contains
    /// multiple headings that all have the same text, these headings are not represented in this
    /// dictionary at all, to avoid confusion.
    /// </summary>
    private readonly Dictionary<string, HeadingRegion> _headingRegionsByText = MakeHeadingRegionsByTextDictionary(regions);

    /// <summary>
    /// Based on an <c>IEnumerable&lt;Region&gt;</c>, build a dictionary for finding instances of
    /// <c>HeadingRegion</c>, keyed on heading text.  Heading text is the text a human reads in the
    /// rendered Markdown document.  When a document contains multiple headings that all have the
    /// same text, none of these colliding headings are represented in the returned dictionary.
    /// This prevents Mdfmt from making assumptions that would be seen by the user as incorrect
    /// or strange.  Its a "first do no harm" sort of approach.
    /// </summary>
    /// <param name="regions">regions of a Markdown file, including the headings</param>
    /// <returns>Dictionary for looking up heading regions by heading text.</returns>
    private static Dictionary<string, HeadingRegion> MakeHeadingRegionsByTextDictionary(IEnumerable<Region> regions)
    {
        // The goal is to build and return this.
        Dictionary<string, HeadingRegion> headingRegionsByText = [];

        // This heading text has to be excluded, because multiple sections share the same heading text.
        HashSet<string> excludedHeadingText = [];

        foreach (HeadingRegion headingRegion in new RegionEnumerable<HeadingRegion>(regions))
        {
            string headingText = headingRegion.HeadingText;
            if (!headingRegionsByText.ContainsKey(headingText))
            {
                // The proposed key is not already in the dictionary.
                // Only use it if it has not been excluded already.
                if (!excludedHeadingText.Contains(headingText))
                {
                    headingRegionsByText[headingText] = headingRegion;
                }
            }
            else
            {
                // Key collision: Don't use this key.
                headingRegionsByText.Remove(headingText);
                excludedHeadingText.Add(headingText);
            }
        }

        return headingRegionsByText;
    }

    /// <summary>
    /// Try to find a <c>HeadingRegion</c> based on lookup by heading text.
    /// </summary>
    /// <param name="headingText">
    /// The text of the heading, as it appears in the rendered Markdown document.
    /// </param>
    /// <param name="headingRegion">Output parameter that returns the <c>&lt;HeadingRegion&gt;</c> when found.</param>
    /// <returns><c>bool</c> indicating whether <c>HeadingRegion</c> was found</returns>
    public bool TryGetHeadingRegion(string headingText, out HeadingRegion headingRegion) =>
        _headingRegionsByText.TryGetValue(headingText, out headingRegion);
}