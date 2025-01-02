using CommandLine;
using System.Text.Json;

namespace Mdfmt.Options;

//NOTE: This class must be in sync with the implementation of MdfmtOptions, which builds on this class.

internal class CommandLineOptions
{
    [Option('f', "flavor", Default = Flavor.Common, HelpText = "Flavor of Markdown formatting.  One of: [Common, Azure].")]
    public Flavor Flavor { get; set; }

    [Option('h', "heading-numbers", HelpText = "Whether to include heading numbers.  When specified, one of: [none, 1., 1].  Use none to remove heading numbers.  Use 1. or 1 to include heading numbers either with or without a trailing period.  If option omitted, no changes to heading numbers.")]
    public string HeadingNumbering { get; set; }

    [Option('t', "toc-threshold", Default = 3, HelpText = "The minimum number of entries for which to include TOC.  0 turns off/removes TOC.")]
    public int TocThreshold { get; set; }

    [Option("newline-strategy", HelpText = "One of: [Unix, Windows, PreferUnix, PreferWindows].  Preferred options do nothing unless file has mixture of newline styles.  If option omitted, no changes to existing newlines, and any new newlines introduced by Mdfmt follow the predominant style of the current file.")]
    public NewlineStrategy? NewlineStrategy { get; set; }

    [Option('r', "recursive", Default = false, HelpText = "Process .md files recursively in all subfolders.")]
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
