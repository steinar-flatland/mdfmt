using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Mdfmt.Model;

/// <summary>
/// An ordered sequence of regions comprising the content of a Markdown file.
/// </summary>
/// <param name="regions"></param>
internal class Regions
(
    List<Region> regions,
    string filePath,
    string cpath,
    string newlineSequence,
    Region firstToc,
    int firstHeadingIndex,
    bool modified = false
) : IEnumerable<Region>
{
    /// <summary>
    /// Regions of the Markdown file.
    /// </summary>
    private readonly List<Region> _regions = regions;

    /// <summary>
    /// Path of Markdown file from which this data structure was loaded.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// Canonical relative file path, or cpath.
    /// </summary>
    public string Cpath { get; } = cpath;

    /// <summary>
    /// Simple file name of the Markdown file, without any path information.
    /// </summary>
    public string FileName { get; } = Path.GetFileName(filePath);

    /// <summary>
    /// The newline sequence to use when assembling the content of the document.
    /// </summary>
    public string NewlineSequence { get; } = newlineSequence;

    /// <summary>
    /// Return a new Headings data structure with the headings of this document.
    /// </summary>
    public Headings Headings
    {
        get
        {
            // Dictionary that maps a string of the form <c>"{cpath}|{headingText}"</c> to a single <c>Heading</c>.
            // (cpath is the canonical relative file path of the file being loaded.)
            Dictionary<string, Heading> headings = [];

            // Ordered sequence of Heading in the document being loaded.
            List<Heading> headingsList = [];

            foreach (Region region in this)
            {
                if (region.IsHeading)
                {
                    string mutableContent = region.MutableContent.TrimStart();
                    int level = mutableContent.TakeWhile(c => c == '#').Count() - 1;
                    string headingText = mutableContent.TrimStart('#').Trim();
                    Heading heading = new(Cpath, level, headingText);
                    string key = Headings.MakeKey(Cpath, heading.Text);
                    headings[key] = heading; // on key repeat, last one wins
                    headingsList.Add(heading);
                }
            }

            // In this use case, this dictionary is not so interesting simply mapping a single Cpath key,
            // which identifies this document being loaded, to the list of heading of this document.
            // We do this because of the flexibility of the Headings data structure being instantiated
            // below, which is flexible enough to load the headings from multiple documents at once,
            // which could be needed for other use cases.
            Dictionary<string, List<Heading>> documentHeadings = new() { { Cpath, headingsList } };

            return new Headings(headings, documentHeadings);
        }
    }

    /// <summary>
    /// Backing storage for the FirstToc property.  If there is no table of contents this is null.
    /// If for some reason there are multiple tables of contents, this is the first one.
    /// </summary>
    private Region _firstToc = firstToc;

    /// <summary>
    /// Whether this Markdown file has a table of contents.
    /// </summary>
    public bool HasToc => _firstToc != null;

    /// <summary>
    /// The (first) table of contents of this Markdown file.  Reading this property when there is no table
    /// of contents results in an <c>InvalidOperationException</c>.  See also the HasToc property.
    /// </summary>
    public Region FirstToc
    {
        get
        {
            if (_firstToc == null)
                throw new InvalidOperationException();
            return _firstToc;
        }
    }

    /// <summary>
    /// The 0-based region index of the first heading, or -1 if there are no headings.
    /// </summary>
    private readonly int _firstHeadingIndex = firstHeadingIndex;

    /// <summary>
    /// Whether there is at least one heading in this Markdown file.
    /// </summary>
    public bool HasFirstHeading => _firstHeadingIndex != -1;

    /// <summary>
    /// Insert a table of contents after the first heading.  Add a blank line between the heading and the new TOC.
    /// An <c>InvalidOperationException</c> will occur if there is already a TOC, or if it is not the case that
    /// there is a first heading to insert after.
    /// </summary>
    /// <param name="tocContent">string with content for the TOC</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void InsertTocAfterFirstHeading(string tocContent)
    {
        if (HasToc || !HasFirstHeading)
            throw new InvalidOperationException();
        AtomicRegion toc = new(tocContent, isMutable: true, isHeading: false, isToc: true);
        // Insert TOC right after first heading.
        _regions.Insert(_firstHeadingIndex + 1, toc);
        // Be sure to incorporate the new TOC into the state of this class
        _firstToc = toc;
        // Insert blank line right before the new TOC.
        _regions.Insert(_firstHeadingIndex + 1, new AtomicRegion(string.Empty, isMutable: true));
        _modified = true;
    }

    public string Content => string.Join(NewlineSequence, _regions.Select(region => region.Content));

    /// <summary>
    /// <para>
    /// This state helps back the Modified property of the Regions data structure.  Regions is
    /// considered to be modified if this flag is true OR if any modified flag in one of the
    /// Region objects in _regions is true.
    /// </para>
    /// <para>
    /// This state captures the modified status of Regions right on initial construction.
    /// Sometimes a newly constructed Regions is already modified, because the user intends
    /// to write it out in a different way, with different newline characters.
    /// </para>
    /// <param>
    /// This flag is also used to notice changes that are not about editing an existing region,
    /// such as inserting new regions, for example, InsertTocAfterFirstHeading().
    /// </param>
    /// </summary>
    private bool _modified = modified;

    /// <summary>
    /// Whether this data structure has been modified since it was loaded.
    /// </summary>
    public bool Modified => _modified || _regions.Any(region => region.IsModified);

    public IEnumerator<Region> GetEnumerator()
    {
        return _regions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Return a count of the number of top-level regions.
    /// (Does not count the atomic regions in composite regions.)
    /// </summary>
    public int Count => _regions.Count;
}
