using CommandLine;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mdfmt.Options;

//NOTE: This class must be in sync with the implementation of MdfmtOptions, which builds on this class.

internal class CommandLineOptions
{
    [Option('e', "environment", HelpText = "Environment name.  When specified, Mdfmt insists on a file, mdfmt.{environment}.json, in the processing root, and settings provided there override and/or extend the optional base configuration from either file mdfmt.json or .mdfmt (.mdfmt is deprecated).  When omitted, the mdfmt.{environment}.json file is not used.")]
    public string Environment { get; set; }

    [Option('f', "flavor", HelpText = "Formatting option: Flavor of link slugification.  When specified, one of: [Common, Azure], ensuring in-document links, including in the TOC, are up to date for the flavor.  When omitted, in-document links are not updated.  See also the related flag --flavor-xdoc, which can be specified with -f, ensuring that cross-document links are updated for flavor as well.")]
    public Flavor? Flavor { get; set; }

    [Option('h', "heading-numbers", HelpText = "Formatting option: Type of heading numbers, or none.  When specified, one of: [1., 1, none].  Use 1. or 1 to include heading numbers either with or without a trailing period.  Use none to remove heading numbers.  When omitted, no changes are made to heading numbers.  If -h is provided, then -f is required, so any incoming links to renumbered headings can be updated.")]
    public string HeadingNumbering { get; set; }

    [Option('l', "line-numbering-threshold", HelpText = "Formatting option: The minimum number of lines in a fenced code block, for line numbering.  When specified, if the line count of a fenced code block meets or exceeds the threshold, ensures that each line starts with a line number, and if the line count is below the threshold, ensures no line numbers.  0 actively removes any line numbers from fenced code blocks.  When omitted, no edits to line numbers in fenced code blocks.")]
    public int? LineNumberingThreshold { get; set; }

    [Option("newline-strategy", HelpText = "Formatting option: Strategy for maintaining newlines.  When specified, one of: [Unix, Windows, PreferUnix, PreferWindows].  Preferred options do nothing unless the file has a mixture of newline styles.  When omitted, no changes are made to existing newlines, and any new newlines introduced by Mdfmt follow the predominant style of the current file.")]
    public NewlineStrategy? NewlineStrategy { get; set; }

    [Option('t', "toc-threshold", HelpText = "Formatting option: The minimum number of headings for which to include a TOC.  When specified, if the number of headings in the document meets or exceeds the threshold, ensures that a TOC is added or updated, and if the number of headings is below the threshold, ensures removal of the TOC.  0 actively removes any TOC.  When omitted, threshold-based TOC maintenance does not occur; however, a pre-existing TOC can still be maintained if -f is specified in this case.  Note that a value > 0 requires -f, so that TOC generation knows how to make link destinations.")]
    public int? TocThreshold { get; set; }

    [Option("audit-links", Default = false, HelpText = "Flag.  Whether to perform a link audit, a different mode of Mdfmt operation that checks all in-document and cross-document links and prints a report to the console.  Useful for finding and fixing broken links.  This is a read-only operation.")]
    public bool AuditLinks { get; set; }

    [Option("flavor-xdoc", Default = false, HelpText = "Flag.  When specified in combination with -f, additional processing occurs on flavor application:  Ensure that resolvable cross-document links are formatted according to the flavor, and report unresolvable cross-document links for investigation.  Note that this requires an extra scan of all Markdown files under the processing root.  This flag is not useful unless combined with -f.")]
    public bool FlavorXdoc { get; set; }

    [Option('r', "recursive", Default = false, HelpText = "Flag.  Process .md files recursively in all subfolders of the target path.  This option is ignored if the target path indicates a specific file, not a directory.")]
    public bool Recursive { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Flag.  Whether to use verbose output.")]
    public bool Verbose { get; set; }

    [Value(0, Default = ".", HelpText = "The target path that Mdfmt is being asked to process, either a single .md file or a directory that contains the .md files to process.  This path may be either absolute or relative to the current working directory where the program was started.")]
    public string TargetPath { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, Constants.JsonSerializerOptions);
    }

    /// <summary>
    /// Based on the target path positional argument passed to the command line and the --recursive (-r) option, 
    /// determine file paths of Markdown files to process.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable"/> exposing an enumerator for 0 or more file paths to Markdown files.
    /// Note that these could be expressed as either relative to the current working directory or as absolute.
    /// (It depends on whether the target path was expressed as relative or absolute.)
    /// </returns>
    [JsonIgnore]
    public IEnumerable<string> MarkdownFilePaths
    {
        get
        {
            // _options.TargetPath has already been verified to be either a specific Markdown file or a directory.
            if (!Directory.Exists(TargetPath))
            {
                // It's a specific file.  Return just the path of that specific file.
                yield return TargetPath;
            }
            else if (Recursive)
            {
                // All Markdown files in target directory, recursively.
                foreach (var file in Directory.EnumerateFiles(TargetPath, Constants.MdWildcard, SearchOption.AllDirectories))
                {
                    yield return file;
                }
            }
            else
            {
                // Markdown files in target directory, non-recursively.
                foreach (var file in Directory.EnumerateFiles(TargetPath, Constants.MdWildcard, SearchOption.TopDirectoryOnly))
                {
                    yield return file;
                }
            }
        }
    }
}
