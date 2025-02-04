using Mdfmt.Generators.Links;
using System;
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
internal class MdStruct
{
    public MdStruct(
        string filePath,
        List<Region> regions,
        bool isModified,
        NewlineRegion newlineRegion
    )
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        _regions = regions;
        _isModified = isModified;
        _newlineRegion = newlineRegion;
        LinkRegions = new RegionEnumerable<LinkRegion>(regions);
        HeadingRegions = new RegionEnumerable<HeadingRegion>(regions);
        TocRegions = new RegionEnumerable<TocRegion>(regions);
        FencedRegions = new RegionEnumerable<FencedRegion>(regions);
        _headingRegionDictionary = MakeHeadingRegionDictionary();
    }

    /// <summary>
    /// Path of Markdown file from which this data structure was loaded.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Simple file name of the Markdown file, without any path information.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Regions of the Markdown file.
    /// </summary>
    private readonly List<Region> _regions;

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
    private bool _isModified;

    /// <summary>
    /// Whether this data structure has been modified since it was loaded.
    /// </summary>
    public bool IsModified => _isModified || _regions.Any(r => r.IsModified);

    /// <summary>
    /// The newline region being used in this file.
    /// </summary>
    private readonly NewlineRegion _newlineRegion;

    /// <summary>
    /// String containing the newline character or sequence being used for line termination.
    /// </summary>
    public string Newline => _newlineRegion.Content;

    /// <summary>
    /// Dictionary keyed on both heading text and various styles of link destinations, leading to 
    /// values that are heading regions.
    /// </summary>
    private readonly Dictionary<string, HeadingRegion> _headingRegionDictionary;

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
    public IEnumerable<LinkRegion> LinkRegions { get; }

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
    public IEnumerable<HeadingRegion> HeadingRegions { get; }

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
    public IEnumerable<TocRegion> TocRegions { get; }

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
    /// Enumerate the regions of type <c>FencedRegion</c>.
    /// </summary>
    public IEnumerable<FencedRegion> FencedRegions { get; }

    /// <summary>
    /// The number of fenced regions.
    /// </summary>
    public int FencedRegionCount => FencedRegions.Count();

    /// <summary>
    /// Whether this Markdown document has any fenced regions.
    /// </summary>
    public bool HasFencedRegion => FencedRegions.Any();

    /// <summary>
    /// <para>
    /// If there is no TOC, insert one after the first heading.
    /// </para>
    /// <para>
    /// When inserting a TOC, include a blank line between the heading and the new TOC. An
    /// <c>InvalidOperationException</c> will occur if TOC insertion is required and it is not the
    /// case that there is a first heading to insert after.
    /// </para>
    /// </summary>
    /// <param name="tocContent">
    /// String with content for the TOC.
    /// </param>
    /// <exception cref="InvalidOperationException"/>
    public void AddToc(string tocContent)
    {
        if (HasToc)
        {
            return;
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
    /// If there is a TOC, delete it.
    /// </summary>
    public void DeleteToc()
    {
        int tocIndex = _regions.FindIndex(r => r is TocRegion);
        if (tocIndex < 0) return; // No TOC to delete.
        int numNewlinesToRemove = Math.Min(2, ConsecutiveNewlinesBefore(tocIndex));
        _regions.RemoveRange(tocIndex - numNewlinesToRemove, numNewlinesToRemove + 1);
        _isModified = true;
    }

    /// <summary>
    /// Get a count of the number of consecutive newlines that immediately precede an index of <c>_regions</c>.
    /// </summary>
    /// <param name="i">Index into <c>_regions</c>.</param>
    /// <returns>A nonnegative integer.</returns>
    private int ConsecutiveNewlinesBefore(int i)
    {
        int count = 0;
        for (int j = i - 1; j >= 0; j--)
        {
            if (_regions[j] is NewlineRegion)
            {
                count++;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    /// <summary>
    /// The content of this Markdown file, reassembled from the regions of the <c>MdStruct</c> data structure.
    /// </summary>
    public string Content => string.Concat(_regions.Select(r => r.Content));

    /// <summary>
    /// <para>
    /// Make and return a new dictionary for finding instances of <c>HeadingRegion</c>, keyed on
    /// both heading text and on link destinations that can target each heading, based on the
    /// state of the headings in this MdStruct.  Heading text is the text a human reads in the
    /// rendered Markdown document.  Link destinations that are supported include all the in
    /// document link formats that Mdfmt supports.
    /// </para>
    /// <para>
    /// Note: When a document contains multiple headings that all have the same text, none of these
    /// colliding headings are represented in the returned dictionary.  This prevents Mdfmt from
    /// making assumptions that would be seen by the user as incorrect or strange.  Its a "first do
    /// no harm" sort of approach.
    /// </para>
    /// </summary>
    /// <returns>
    /// Dictionary for looking up heading regions by heading text and by link destination.
    /// </returns>
    private Dictionary<string, HeadingRegion> MakeHeadingRegionDictionary()
    {
        // The goal is to build and return this.
        Dictionary<string, HeadingRegion> headingRegions = [];

        // This heading text has to be excluded, because multiple sections share the same heading text.
        HashSet<string> excludedHeadingText = [];

        // Make a list of link destination generators, one per supported link format.
        List<ILinkDestinationGenerator> linkDestinationGenerators = LinkDestinationGeneratorFactory.ManufactureOneOfEach();

        foreach (HeadingRegion headingRegion in new RegionEnumerable<HeadingRegion>(_regions))
        {
            string headingText = headingRegion.HeadingText;
            if (!headingRegions.ContainsKey(headingText))
            {
                // The proposed key is not already in the dictionary.
                // Only use it if it has not been excluded already.
                if (!excludedHeadingText.Contains(headingText))
                {
                    headingRegions[headingText] = headingRegion;
                    foreach (ILinkDestinationGenerator generator in linkDestinationGenerators)
                    {
                        string linkDestination = generator.GenerateLinkDestination(FileName, headingText);
                        headingRegions[linkDestination] = headingRegion;
                    }
                }
            }
            else
            {
                // Key collision: Don't use this key.
                headingRegions.Remove(headingText);
                foreach (ILinkDestinationGenerator generator in linkDestinationGenerators)
                {
                    string linkDestination = generator.GenerateLinkDestination(FileName, headingText);
                    headingRegions.Remove(linkDestination);
                }
                excludedHeadingText.Add(headingText);
            }
        }

        return headingRegions;
    }

    /// <summary>
    /// Try to get a heading region based on a key that is either the text of the heading or an
    /// in-document link destination that targets the heading.  The keys are frozen in time as
    /// they were when this MdStruct was loaded, and do not reflect any updates to the MdStruct
    /// such as changing headings to update heading numbers.  The keys lead to HeadingRegion 
    /// values that *are* up to date with the latest changes.  (Stale keys leading to current
    /// values is useful for updating in-document links.)
    /// </summary>
    /// <param name="key">
    /// A key that is either the text of a heading or a way of expressing an in-document link 
    /// destination that targets the heading, as of the time when this document was first
    /// brought into memory.
    /// </param>
    /// <param name="headingRegion">
    /// On true return value, this is set to the <c>HeadingRegion</c> that was found.
    /// </param>
    /// <returns>Whether a heading region was found for the key.</returns>
    public bool TryGetHeadingRegion(string key, out HeadingRegion headingRegion)
    {
        return _headingRegionDictionary.TryGetValue(key, out headingRegion);
    }
}
