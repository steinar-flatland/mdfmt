using CommandLine;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mdfmt.Options;

//NOTE: This class must be in sync with the implementation of MdfmtOptions, which builds on this class.

internal class CommandLineOptions
{
    [Option('e', "environment", HelpText = "Optional environment name.  When specified, Mdfmt insists on a file named \"mdfmt.{environment}.json\" in the processing root, and provided settings override and/or extend the optional base configuration of either mdfmt.json or .mdfmt (.mdfmt file is deprecated).")]
    public string Environment { get; set; }

    [Option('f', "flavor", HelpText = "Flavor of link slugification.  When specified, one of: [Common, Azure], causing in-document links, including in TOC, to be updated for the flavor.  If this option is omitted, links are not updated.  If the -t option > 0, then -f is required, to inform the flavor of TOC link destinations.  If the -h option is provided, then -f is required, so any incoming links to renumbered headings can be updated.")]
    public Flavor? Flavor { get; set; }

    [Option('h', "heading-numbers", HelpText = "Whether to include heading numbers.  When specified, one of: [none, 1., 1].  Use none to remove heading numbers.  Use 1. or 1 to include heading numbers either with or without a trailing period.  If this option is omitted, no changes are made to heading numbers.  If the -h option is provided, then -f is required, so any incoming links to renumbered headings can be updated.")]
    public string HeadingNumbering { get; set; }

    [Option('t', "toc-threshold", HelpText = "The minimum number of headings for which to include a TOC.  When specified, if the number of headings in the document meets or exceeds the threshold, ensures that a TOC is added or updated, and if the number of headings is below the threshold, ensures removal of the TOC.  0 actively removes any TOC.  If this option is omitted, threshold-based TOC maintenance does not occur; however, a pre-existing TOC can still be maintained if -f is specified in this case.  Note that a value > 0 requires the -f option, so that TOC generation will know how to make link destinations.")]
    public int? TocThreshold { get; set; }

    [Option('l', "line-numbering-threshold", HelpText = "The minimum number of lines in a fenced code block, for line numbering.  When specified, if the line count of a fenced code block meets or exceeds the threshold, ensures that each line starts with a line number, and if the line count is below the threshold, ensures no line numbers.  0 actively removes any line numbers from fenced code blocks.  If this option is omitted, do not edit line numbers in fenced code blocks.")]
    public int? LineNumberingThreshold { get; set; }

    [Option("newline-strategy", HelpText = "Strategy for maintaining newlines.  When specified, one of: [Unix, Windows, PreferUnix, PreferWindows].  Preferred options do nothing unless the file has a mixture of newline styles.  If this option is omitted, no changes are made to existing newlines, and any new newlines introduced by Mdfmt follow the predominant style of the current file.")]
    public NewlineStrategy? NewlineStrategy { get; set; }

    [Option('r', "recursive", Default = false, HelpText = "Process .md files recursively in all subfolders of the target path.  This option is ignored if the target path indicates a specific file, not a directory.")]
    public bool Recursive { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Whether to use verbose output.")]
    public bool Verbose { get; set; }

    [Option("audit-links", Default = false, HelpText = "Whether to perform a link audit, which checks all in-document and cross-document links and prints a report to the console.  Useful for finding and fixing broken links.")]
    public bool AuditLinks { get; set; }

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
