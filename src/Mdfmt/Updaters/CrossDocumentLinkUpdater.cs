using Mdfmt.Generators.Links;
using Mdfmt.Model;
using Mdfmt.Utilities;
using System.Collections.Generic;

namespace Mdfmt.Updaters;

internal class CrossDocumentLinkUpdater(
    ILinkDestinationGenerator linkDestinationGenerator,
    IReadOnlyDictionary<string, string> staleCpathFragmentToUpdatedHeadingText,
    IReadOnlySet<string> ambiguousStaleCpathFragments)
{
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;
    private readonly IReadOnlyDictionary<string, string> _staleCpathFragmentToUpdatedHeadingText = staleCpathFragmentToUpdatedHeadingText;
    private readonly IReadOnlySet<string> _ambiguousStaleCpathFragments = ambiguousStaleCpathFragments;

    public void Update(MdStruct md, bool verbose)
    {
        foreach (LinkRegion linkRegion in md.LinkRegions)
        {
            // Extract parts of the link destination.
            string path = linkRegion.Path;
            string fragment = linkRegion.Fragment;

            // Does the link look like it has a relative path to another document, followed by a fragment specifying a target heading?
            bool isCrossDocumentLink = path.StartsWith('.') && !path.Equals($"./{md.FileName}", System.StringComparison.OrdinalIgnoreCase) && fragment.Length > 0;
            if (!isCrossDocumentLink)
            {
                // Nothing to do for this link.
                continue;
            }

            // Compute key for lookups in _staleCpathFragmentToUpdatedHeadingText and _ambiguousStaleCpathFragments.
            string key = $"{PathUtils.Canonicalize(md.Cpath, path)}{fragment}";

            if (_ambiguousStaleCpathFragments.Contains(key))
            {
                Output.Warn($"{md.FilePath}: The cross-document link {linkRegion.Content} targets a heading of which there were multiple instances in the target file before recent changes to heading numbers.  Mdfmt is not maintaining links to this ambiguous target.  Please check this manually.");
            } else if (_staleCpathFragmentToUpdatedHeadingText.TryGetValue(key, out string updatedHeadingText))
            {
                string destination = _linkDestinationGenerator.GenerateCrossDocumentLinkDestination(path, updatedHeadingText);
                if (linkRegion.Destination != destination)
                {
                    linkRegion.Destination = destination;
                    if (verbose)
                    {
                        Output.Emphasis($"- {md.FilePath}: Updated cross-document link with label [{linkRegion.Label}] to target destination ({linkRegion.Destination})");
                    }
                }
            }
            else
            {
                // Assume that this link region's destination is valid, since it is not in the dictionary indicating it is stale.
                // This is OK; no need to warn.
            }
        }
    }
}
