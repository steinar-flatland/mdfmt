using Mdfmt.Generators.Links;
using Mdfmt.Model;
using System.Collections.Generic;
using System;
using Mdfmt.Loaders;
using Mdfmt.Options;

namespace Mdfmt;

/// <summary>
/// This class audits in-document and cross-document links of Markdown files that occur under the
/// processing root.  It reports broken links, organized by containing file.  It shows a summmary
/// of the links found.  It then returns either a 1 (GeneralError) or 0 (Success) exit code to 
/// the shell.
/// </summary>
internal class LinkAuditor(MdfmtOptions mdfmtOptions, MdStructLoader mdStructLoader, IReadOnlyList<ILinkDestinationGenerator> linkDestinationGenerators)
{
    private readonly MdfmtOptions _options = mdfmtOptions;
    private readonly MdStructLoader _mdStructLoader = mdStructLoader;
    private readonly IReadOnlyList<ILinkDestinationGenerator> _linkDestinationGenerators = linkDestinationGenerators;

    public void AuditLinks()
    {
        // Dictionary keyed on Markdown file cpath leading to set of slugified headings in that file.
        Dictionary<string, HashSet<string>> slugsOfCpaths = LoadSlugsOfCpaths();

        // Counters to aggregate info across all files.
        int fileCount = 0;
        int brokenLinkCount = 0;
        int intactLinkCount = 0;
        int externalLinkCount = 0;

        // Broken LinkRegions (if any) in each file.
        List<LinkRegion> brokenLinkRegions = [];

        // Audit
        foreach (string filePath in _options.AllMarkdownFilePaths)
        {
            fileCount++;
            brokenLinkRegions.Clear();
            MdStruct md = _mdStructLoader.Load(filePath);

            foreach (LinkRegion linkRegion in md.LinkRegions)
            {
                string destinationCpath = linkRegion.DestinationCpath(md);

                // If destination cpath cannot be determined, the link is external.
                if (destinationCpath == null)
                {
                    externalLinkCount++;
                    continue;
                }

                // Determine if link is intact or broken.
                if (slugsOfCpaths.TryGetValue(destinationCpath, out HashSet<string> slugs))
                {
                    // Target file validated.

                    // If link has a fragment containing a slug, we need to verify it.
                    string slugToVerify = linkRegion.Fragment.TrimStart('#');

                    if (slugToVerify.Length > 0)
                    {
                        if (slugs.Contains(slugToVerify))
                        {
                            // Link is intact
                            intactLinkCount++;
                        }
                        else
                        {
                            // Broken link!
                            brokenLinkCount++;
                            brokenLinkRegions.Add(linkRegion);
                        }
                    }
                    else
                    {
                        // Link goes to a file, but not to a heading in that file.
                        string destination = linkRegion.Destination.Trim();
                        if (destination == "" || destination == "#")
                        {
                            // Assume that the user wants to fix empty link destinations, so treat these as broken.
                            brokenLinkCount++;
                            brokenLinkRegions.Add(linkRegion);
                        }
                        else
                        {
                            intactLinkCount++;
                        }
                    }
                }
                else
                {
                    // Target file could not be validated.  Broken link!
                    brokenLinkCount++;
                    brokenLinkRegions.Add(linkRegion);
                }
            }

            // If this file had broken links, show them.
            if (brokenLinkRegions.Count > 0)
            {
                Output.Info($"{Environment.NewLine}Broken links in ", false, ConsoleColor.Yellow);
                Output.Info(md.FilePath, true, ConsoleColor.Cyan);
                foreach (LinkRegion linkRegion in brokenLinkRegions)
                {
                    Output.Warn($"  {linkRegion.Content}");
                }
            }
        }

        // Final report wtih summary info.
        Output.Info($"{Environment.NewLine}Scanned {fileCount} files.");
        Output.Info($"{intactLinkCount + brokenLinkCount} internal links found.");
        Output.Info($"  Intact:{intactLinkCount,6}");
        Output.Info($"  Broken:{brokenLinkCount,6}");
        Output.Info($"{externalLinkCount} external links found.{Environment.NewLine}");

        // Return either GeneralError or Success exit code.  This could be useful, for example, for a publishing pipeline.
        // This could block publishing, unless links are intact.
        if (brokenLinkCount > 0)
        {
            throw new ExitException(ExitCodes.GeneralError);
        }
        else
        {
            throw new ExitException(ExitCodes.Success);
        }
    }

    /// <summary>
    /// Based on all the Markdown files under the processing root, load a dictionary keyed on cpath
    /// of Markdown file leading to a set of slugified headings in that file.
    /// </summary>
    /// <returns>
    /// Dictionary keyed on Markdown file cpath leading to set of slugified headings in that file.
    /// </returns>
    private Dictionary<string, HashSet<string>> LoadSlugsOfCpaths()
    {
        // Dictionary mapping each cpath to a set of slugified headings.
        Dictionary<string, HashSet<string>> slugsOfCpaths = [];

        // Populate and return slugsOfCpaths.
        foreach (string filePath in _options.AllMarkdownFilePaths)
        {
            MdStruct md = _mdStructLoader.Load(filePath);
            HashSet<string> slugs = [];
            slugsOfCpaths[md.Cpath] = slugs;
            foreach (HeadingRegion headingRegion in md.HeadingRegions)
            {
                foreach (ILinkDestinationGenerator linkDestinationGenerator in _linkDestinationGenerators)
                {
                    string slug = linkDestinationGenerator.SlugifyHeadingText(headingRegion.HeadingText);
                    slugs.Add(slug);
                }
            }
        }
        return slugsOfCpaths;
    }

}
