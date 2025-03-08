using Mdfmt.Generators.Links;
using Mdfmt.Model;
using Mdfmt.Utilities;
using System.Collections.Generic;

namespace Mdfmt.Updaters;

/// <summary>
/// Ensure that cross-document links are up to date.  There are two aspects to this:  (1) If 
/// headings have changed due to adjustments to heading numbers, then all links that target
/// these modified headings must be updated.  This potentially affects both in-document and
/// cross-document links.  (2) If the --flavor-xdoc flag has been specified, meaning that
/// cross-document links should be verified and adjusted for flavor, then update any 
/// cross-document links that are expressed with the wrong flavor.
/// </summary>
/// <param name="linkDestinationGenerator">
/// Knows how to generate an in-document link destination according to a slugification flavor.
/// </param>
/// <param name="staleCpathFragmentToUpdatedHeadingText">
/// Dictionary keyed on a string of the form <c>$"{cpath}{fragment}"</c>.  <c>{cpath}</c> is the
/// canonical relative path, relative to the processing root, of a Markdown file.  <c>{fragment}</c>
/// is a <c>'#'</c> character followed by slugified heading text.  The key is stale, i.e. it is
/// for a heading that has changed because its heading number (which is part of the heading) 
/// changed, e.g. the heading number could have been removed when it was present, added when it
/// was missing, or modified if it already existed.  Each key, with stale information, maps to 
/// the new updated heading text.  The purpose of this data structure is to drive cross-document
/// link updating.
/// </param>
/// <param name="ambiguousStaleCpathFragments">
/// This is a set of keys like the ones that occur in the <c>staleCpathFragmentToUpdatedHeadingText</c>
/// dictionary, but the keys cannot be included in that dictionary because documents had duplicate headings
/// (a Markdown anti-practice) and that would have led to key collisions in
/// <c>staleCpathFragmentToUpdatedHeadingText</c>.  The purpose of saving this information is to 
/// generate warning messages during cross-document link updating, to inform the user of things they
/// must investigate manually.  With well-structured Markdown, this kind of ambiguity won't occur,
/// but we have to be prepared for it.
/// </param>
/// <param name="fragmentToHeadingText">
/// Dictionary of heading slug to the heading text from which the slug was generated,
/// for all headings in all Markdown files within the processing root, for all ways of 
/// slugifying.
/// </param>
internal class CrossDocumentLinkUpdater(
    ILinkDestinationGenerator linkDestinationGenerator,
    IReadOnlyDictionary<string, string> staleCpathFragmentToUpdatedHeadingText,
    IReadOnlySet<string> ambiguousStaleCpathFragments,
    IReadOnlyDictionary<string, string> fragmentToHeadingText)
{
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;
    private readonly IReadOnlyDictionary<string, string> _staleCpathFragmentToUpdatedHeadingText = staleCpathFragmentToUpdatedHeadingText;
    private readonly IReadOnlySet<string> _ambiguousStaleCpathFragments = ambiguousStaleCpathFragments;
    private readonly IReadOnlyDictionary<string, string> _fragmentToHeadingText = fragmentToHeadingText;

    public void Update(MdStruct md, bool verbose)
    {
        bool fileNameDisplayed = false;

        foreach (LinkRegion linkRegion in md.LinkRegions)
        {
            // Skip links that are not cross-document.
            if (linkRegion.GetLinkType(md.FileName) != LinkType.CrossDocument)
            {
                continue;
            }

            // By definition of LinkType.CrossDocument, here are some things we know about linkRegion.Path
            // and how we are viewing things.
            // - This is a non-null, non-empty, non-whitespace string (already verified).
            // - It does not start with '/' so it does not look like an absolute path (already verified).
            // - It is not a URL like http:// or https:// (already verified).
            // - It always ends in ".md" (already verified).
            // - It indicates a Markdown file that is a different file from which MdStruct md was loaded.
            //   We have verified this is not $"{md.FileName}" and it is not $"./{md.FileName}"
            //   (already verified).
            // - It could be just a simple file name, or there could be /-separated path segments before
            //   the file name (convention for paths we are supporting).
            // - It might start with "./" or "../", or it might not (truism).
            // - If it does not start with "." we think of an initial "./" as being implicit (point of view/convention).
            string path = linkRegion.Path;

            string canonicalPath = PathUtils.Canonicalize(md.Cpath, path);

            // Warn that a link is broken because it is not possible to canonicalize the path.
            // This occurs if the relative path goes up too many directories and "busts out"
            // of the processing root.
            if (canonicalPath == null)
            {
                Output.Warn($"{md.FilePath}: Invalid link: {linkRegion.Content}");
                continue;
            }

            string fragment = linkRegion.Fragment;

            // If there is no fragment, we don't have to be concerned with changes to the fragment
            // due to section renumbering or flavor changes.
            if (fragment.Length == 0)
            {
                continue;
            }

            // Compute key for lookups in _staleCpathFragmentToUpdatedHeadingText and _ambiguousStaleCpathFragments.
            string key = $"{canonicalPath}{fragment}";

            // Warn if key is ambiguous.
            if (_ambiguousStaleCpathFragments.Contains(key))
            {
                // Warn that a link targets a heading of which there were multiple instances in the
                // target file, before recent changes to heading numbers. Mdfmt is unsure how to
                // maintain this, and the user needs to check it manually.
                Output.Warn($"{md.FilePath}: Ambiguous link: {linkRegion.Content}");
                continue;
            }

            // If the key targets a heading that has changed, ensure the link is in the correct state.
            if (_staleCpathFragmentToUpdatedHeadingText.TryGetValue(key, out string updatedHeadingText))
            {
                string destination = _linkDestinationGenerator.GenerateCrossDocumentLinkDestination(path, updatedHeadingText);
                if (linkRegion.Destination != destination)
                {
                    linkRegion.Destination = destination;
                    if (verbose)
                    {
                        if (!fileNameDisplayed)
                        {
                            Output.Emphasis($"- {md.FilePath}: Updated cross-document links:");
                            fileNameDisplayed = true;
                        }
                        Output.Emphasis($"  {linkRegion.Content}");
                    }
                }
                continue;
            }

            // _fragmentToHeadingText is only set if --flavor-xdoc.  This option is for ensuring
            // that resolvable cross-document links are formatted according to the right flavor.
            if (_fragmentToHeadingText == null) continue;

            if (_fragmentToHeadingText.TryGetValue(fragment, out string headingText))
            {
                string destination = _linkDestinationGenerator.GenerateCrossDocumentLinkDestination(path, headingText);
                if (linkRegion.Destination != destination)
                {
                    linkRegion.Destination = destination;
                    if (verbose)
                    {
                        if (!fileNameDisplayed)
                        {
                            Output.Emphasis($"- {md.FilePath}: Updated cross-document links:");
                            fileNameDisplayed = true;
                        }
                        Output.Emphasis($"  {linkRegion.Content}");
                    }
                }
            }
            else
            {
                Output.Warn($"{md.FilePath}: Broken link: {linkRegion.Content}");
            }
        } // end foreach link region
    }
}
