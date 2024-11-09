using System.Collections.Generic;

namespace Mdfmt.Model;

/// <summary>
/// <para>
/// Headings that have been loaded from one or more Markdown files.
/// </para>
/// <para>
/// There are two concepts that are important for understanding how to look up headings in this
/// data structure and associated use cases:  (1) canonical relative file path and (2) heading text.
/// </para>
/// <para>
/// Canonical relative file path:  Each Markdown file has a canonical relative file path.  That is
/// quite long to say, so we coin the term "cpath" to mean this.  A cpath always starts with "./",
/// where the "." refers to the directory path option that was passed to the mdfmt program.  If
/// instead of a directory, the path provided to mdfmt is for a single, specific .md file, then
/// the cpath still starts with "./", and the "." represents the directory where this one file
/// happens to be.
/// </para>
/// <para>
/// Heading text: This text reflects exactly how the heading is displayed to the user.  If in the
/// Markdown file the heading had embedded HTML comments, or there were leading or trailing spaces,
/// these have been removed.
/// </para>
/// <para>
/// It is worth noting that there can be relative paths in Markdown files that are not cpaths.
/// Here's a simple example:  The root path given to mdfmt is a directory A with subdirectories
/// B and C.  The B directory has a file b.md, and the C directory has a file c.md.  There is a
/// cross document link from b.md to c.md, where b.md refers to file c.md as: <c>../C/c.md</c>.
/// While this is a relative path, it is not a cpath.  The cpath of any file must be relative
/// to the mdfmt root path, in this case <c>./C/c.md</c>.
/// </para>
/// <para>
/// There are two scenarios for looking up headings in this data structure:  (1) for TOC generation
/// and (2) for link destination formatting.
/// </para>
/// <para>
/// TOC generation:  For table of contents (TOC) generation, an ordered sequence of all of the
/// headings from a single document is needed.  This can be looked up using a cpath as the key,
/// resulting in a List of Heading.
/// </para>
/// <para>
/// Link destination formatting: A Markdown link is of the form <c>[label](destination)</c>.
/// Further, the destination is composed of two parts, some kind of path or URL and and a
/// fragment (part after a '#') indicating a heading target.  Mdfmt does not change the
/// path part, but it does update the fragment part, because the details of the fragment
/// vary by platform such as Azure, VsCode, etc.  Mdfmt will only update the fragment if
/// it can first understand that the path part of the link destination, coupled with the
/// link label, target a Markown heading in scope of the mdfmt run.  To check this, Mdfmt
/// checks to be sure the destintation starts with either "./" or "../" (indicating a
/// relative link), and canonicalizes this to make a cpath.  Then, a lookup in this
/// Headings data structure by the combination of cpath and link label is made, to see if it can
/// be matched to heading text.  So in short, for this use case the lookup is by the
/// combination of cpath and heading text.
/// </para>
/// </summary>
internal class Headings(Dictionary<string, Heading> headings, Dictionary<string, List<Heading>> documentHeadings)
{
    /// <summary>
    /// Dictionary that maps a string of the form <c>"{cpath}|{headingText}"</c> to a single <c>Heading</c>.
    /// </summary>
    private readonly Dictionary<string, Heading> _headings = headings;

    /// <summary>
    /// Dictionary that maps a cpath to a List of 0 or more <c>Heading</c>.  The order of the headings
    /// in this list matches their order in the document.
    /// </summary>
    private readonly Dictionary<string, List<Heading>> _documentHeadings = documentHeadings;

    /// <summary>
    /// Try to find the ordered headings of a cpath.
    /// </summary>
    /// <param name="cpath">Canonical relative file path, or cpath</param>
    /// <param name="headings">Output parameter that is set to the found headings or null if not found.</param>
    /// <returns>true if the headings are found, false otherwise</returns>
    public bool TryGetValue(string cpath, out List<Heading> headings)
    {
        return _documentHeadings.TryGetValue(cpath, out headings);
    }

    /// <summary>
    /// Convenience method to make a key of the form <c>"{cpath}|{headingText}"</c>.
    /// </summary>
    /// <param name="cpath">Canonical relative file path, or cpath</param>
    /// <param name="headingText">heading text</param>
    /// <returns>A key for finding a specific heading.</returns>
    public static string MakeKey(string cpath, string headingText) => $"{cpath}|{headingText}";

    /// <summary>
    /// Try to find a heading by the combination of cpath and headingText.
    /// </summary>
    /// <param name="cpath">Canonical relative file path, or cpath</param>
    /// <param name="headingText">heading text</param>
    /// <param name="heading">Output parameter that is set to the found Heading or null if not found.</param>
    /// <returns>true if the Heading is found, false if not found</returns>
    public bool TryGetValue(string cpath, string headingText, out Heading heading)
    {
        string key = MakeKey(cpath, headingText);
        return _headings.TryGetValue(key, out heading);
    }

    /// <summary>
    /// Get a heading by the kind of key that the MakeKey() method produces.
    /// Use this for safe lookups, when you already know the key exists.
    /// </summary>
    /// <param name="key">A key of the form <c>"{cpath}|{headingText}"</c>.</param>
    /// <returns>Heading</returns>
    public Heading this[string key]
    {
        get
        {
            return _headings[key];
        }
    }

    /// <summary>
    /// Return a count of the number of headings.  
    /// </summary>
    public int Count => _headings.Count;

    /// <summary>
    /// Return the keys of the dictionary.  This lets you write code like
    /// <c>foreach (string key in headings.Keys) { ... }</c>
    /// </summary>
    public Dictionary<string, Heading>.KeyCollection Keys => _headings.Keys;

    /// <summary>
    /// Return the values of the dictionary.  This lets you write code like
    /// <c>foreach (Heading heading in headings.Values) { ... }</c>
    /// </summary>
    public Dictionary<string, Heading>.ValueCollection Values => _headings.Values;
}
