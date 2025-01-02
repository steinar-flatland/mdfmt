using CommandLine;
using System.Text.Json;

namespace Mdfmt.Options;

//NOTE: This class must be in sync with the implementation of MdfmtOptions, which builds on this class.

internal class CommandLineOptions
{
    [Option('f', "flavor", HelpText = "Flavor of link slugification.  When specified, one of: [Common, Azure], and destinations of in-document links, including those in both the body of the document and in any pre-existing TOC, are updated for the flavor.  If this option is omitted, links are not updated.  Note that if the value of the -t (--toc-threshold) option > 0, this option is required, to inform the flavor of TOC link destinations.")]
    public Flavor? Flavor { get; set; }

    [Option('h', "heading-numbers", HelpText = "Whether to include heading numbers.  When specified, one of: [none, 1., 1].  Use none to remove heading numbers.  Use 1. or 1 to include heading numbers either with or without a trailing period.  If this option is omitted, no changes are made to heading numbers.")]
    public string HeadingNumbering { get; set; }

    [Option('t', "toc-threshold", HelpText = "The minimum number of headings for which to include a table of contents (TOC).  When specified, if the number of headings in the document meets or exceeds the threshold, ensures that a TOC is added or updated, and if the number of headings is below the threshold, ensures removal of the TOC.  0 actively removes any TOC.  If this option is omitted, threshold-based TOC maintenance does not occur; however, a pre-existing TOC can still be maintained if -f (--flavor) is specified in this case.  Note that a value > 0 requires the -f option, to know how to create TOC link destinations.")]
    public int? TocThreshold { get; set; }

    [Option("newline-strategy", HelpText = "Strategy for maintaining newlines.  When specified, one of: [Unix, Windows, PreferUnix, PreferWindows].  Preferred options do nothing unless the file has a mixture of newline styles.  If this option is omitted, no changes are made to existing newlines, and any new newlines introduced by Mdfmt follow the predominant style of the current file.")]
    public NewlineStrategy? NewlineStrategy { get; set; }

    [Option('r', "recursive", Default = false, HelpText = "Process .md files recursively in all subfolders.  This option is ignored if the positional argument is a file, not a directory.")]
    public bool Recursive { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Whether to use verbose output.")]
    public bool Verbose { get; set; }

    [Value(0, Default = ".", HelpText = "The directory in which to process .md files, or a specific .md file.")]
    public string Path { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, Constants.JsonSerializerOptions);
    }
}
