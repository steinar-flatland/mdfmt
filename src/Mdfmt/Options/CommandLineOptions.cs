using CommandLine;

namespace Mdfmt.Options;

public class CommandLineOptions
{
    [Option('p', "platform", Default = Platform.Azure, HelpText = "Target platform for Markdown formatting.  One of: [Azure, VsCode].")]
    public Platform Platform { get; set; }

    [Option('n', "heading-numbers", Default = "none", HelpText = "Whether to include heading numbers.  One of: [none, 1., 1].  Use 1. or 1 to include heading numbers. The name of the \"1\" options indicates whether generated numbers will end in a period.")]
    public string HeadingNumbering { get; set; }

    [Option('r', "recursive", Default = false, HelpText = "Process .md files recursively in all subfolders.")]
    public bool Recursive { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Whether to use verbose output.")]
    public bool Verbose { get; set; }

    [Value(0, Default = ".", HelpText = "The directory in which to process .md files, or a specific .md file.")]
    public string Path { get; set; }

    [Option("minimum-entry-count", Default = 2, HelpText = "The minimum number of entries for which to include TOC.  0 turns off/removes TOC.")]
    public int MinimumEntryCount { get; set; }

    [Option("newline-strategy", Default = NewlineStrategy.PreferWindows, HelpText = "One of: [Unix, Windows, PreferUnix, PreferWindows].  Preferred options respect the file and take effect only if the file has a mixture.")]
    public NewlineStrategy NewlineStrategy { get; set; }
}
