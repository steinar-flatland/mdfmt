using System;
using System.Collections.Generic;
using System.IO;
using Mdfmt;

namespace Integration.Mdfmt;

[TestFixture]
public class ProgramTests
{
    private Program _program;

    private static readonly string _workDir = "./Work";
    private static readonly string _markdownDir = "./Markdown";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Directory.CreateDirectory(_workDir);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Directory.Delete(_workDir, true);
    }

    [SetUp]
    public void Setup()
    {
        _program = new Program();
    }

    private static readonly IEnumerable<TestCaseData> _testCases =
    [
        new TestCaseData("Empty.md", Array.Empty<string>(), "Empty.md").
        SetName("Empty.1: Given an empty file, When no arguments, Then empty file."),

        new TestCaseData("Empty.md", new string[] {"-f", "azure", "-h", "1", "-t", "2", "--newline-strategy", "PreferWindows", "-v"}, "Empty.md").
        SetName("Empty.2: Given an empty file, When several arguments, Then empty file."),

        new TestCaseData("Title.md", new string[] {"-t", "1"}, "Title_toc.md").
        SetName("TOC.1: Given a file with a title only, When mdfmt -t 1, Then a TOC is added."),

        new TestCaseData("Title.md", new string[] {"--toc-threshold", "1"}, "Title_toc.md").
        SetName("TOC.2: Given a file with a title only, When mdfmt --toc-threshold 1, Then a TOC is added."),

        new TestCaseData("Title.md", new string[] {"-t", "0"}, "Title.md").
        SetName("TOC.3: Given a file with a title only, When mdfmt -t 0, Then no change."),

        new TestCaseData("Title_toc.md", new string[] {"-t", "0"}, "Title.md").
        SetName("TOC.4: Given a file with a title and TOC, When mdfmt -t 0, Then the TOC is removed."),

        new TestCaseData("Title_toc.md", new string[] {"--toc-threshold", "0"}, "Title.md").
        SetName("TOC.5: Given a file with a title and TOC, When mdfmt --toc-threshold 0, Then the TOC is removed."),

        new TestCaseData("Title_toc.md", new string[] {"-t", "1"}, "Title_toc.md").
        SetName("TOC.6: Given a file with a title and TOC, When mdfmt -t 1, Then no change."),

        new TestCaseData("Title_toc_outdated.md", new string[] {"-t", "1"}, "Title_toc_updated.md").
        SetName("TOC.7: Given a file with an outdated TOC, When mdfmt -t 1, Then the TOC is updated."),

        new TestCaseData("Line-Endings_unix.md", new string[] {"--newline-strategy", "Unix"}, "Line-Endings_unix.md").
        SetName("Line-Endings.01: Given a file with unix line endings, When mdfmt --newline-strategy Unix, Then no change."),

        new TestCaseData("Line-Endings_unix.md", new string[] {"--newline-strategy", "Windows"}, "Line-Endings_windows.md").
        SetName("Line-Endings.02: Given a file with unix line endings, When mdfmt --newline-strategy Windows, Then Windows line endings."),

        new TestCaseData("Line-Endings_unix.md", new string[] {"--newline-strategy", "PreferUnix"}, "Line-Endings_unix.md").
        SetName("Line-Endings.03: Given a file with unix line endings, When mdfmt --newline-strategy PreferUnix, Then no change."),

        new TestCaseData("Line-Endings_unix.md", new string[] {"--newline-strategy", "PreferWindows"}, "Line-Endings_unix.md").
        SetName("Line-Endings.04: Given a file with unix line endings, When mdfmt --newline-strategy PreferWindows, Then no change."),

        new TestCaseData("Line-Endings_windows.md", new string[] {"--newline-strategy", "Unix"}, "Line-Endings_unix.md").
        SetName("Line-Endings.05: Given a file with windows line endings, When mdfmt --newline-strategy Unix, Then Unix line endings."),

        new TestCaseData("Line-Endings_windows.md", new string[] {"--newline-strategy", "Windows"}, "Line-Endings_windows.md").
        SetName("Line-Endings.06: Given a file with windows line endings, When mdfmt --newline-strategy Windows, Then no change."),

        new TestCaseData("Line-Endings_windows.md", new string[] {"--newline-strategy", "PreferUnix"}, "Line-Endings_windows.md").
        SetName("Line-Endings.07: Given a file with windows line endings, When mdfmt --newline-strategy PreferUnix, Then no change."),

        new TestCaseData("Line-Endings_windows.md", new string[] {"--newline-strategy", "PreferWindows"}, "Line-Endings_windows.md").
        SetName("Line-Endings.08: Given a file with windows line endings, When mdfmt --newline-strategy PreferWindows, Then no change."),

        new TestCaseData("Line-Endings_mixed.md", new string[] {"--newline-strategy", "Unix"}, "Line-Endings_mixed_fixed_for_unix.md").
        SetName("Line-Endings.09: Given a file with mixed line endings, When mdfmt --newline-strategy Unix, Then Unix line endings."),

        new TestCaseData("Line-Endings_mixed.md", new string[] {"--newline-strategy", "Windows"}, "Line-Endings_mixed_fixed_for_windows.md").
        SetName("Line-Endings.10: Given a file with mixed line endings, When mdfmt --newline-strategy Windows, Then Windows line endings."),

        new TestCaseData("Line-Endings_mixed.md", new string[] {"--newline-strategy", "PreferUnix"}, "Line-Endings_mixed_fixed_for_unix.md").
        SetName("Line-Endings.11: Given a file with mixed line endings, When mdfmt --newline-strategy PreferUnix, Then Unix line endings."),

        new TestCaseData("Line-Endings_mixed.md", new string[] {"--newline-strategy", "PreferWindows"}, "Line-Endings_mixed_fixed_for_windows.md").
        SetName("Line-Endings.12: Given a file with mixed line endings, When mdfmt --newline-strategy PreferWindows, Then Windows line endings."),

        new TestCaseData("Flavor_common.md", new string[] {"-f", "Azure"}, "Flavor_azure.md").
        SetName("Flavor.1: Given a file expressed using Common flavor, When mdfmt -f Azure, Then the file is converted to Azure flavor."),

        new TestCaseData("Flavor_azure.md", new string[] {"-f", "Common"}, "Flavor_common.md").
        SetName("Flavor.2: Given a file expressed using Azure flavor, When mdfmt -f Common, Then the file is converted to Common flavor."),

        new TestCaseData("Flavor_common.md", new string[] {"-f", "Common"}, "Flavor_common.md").
        SetName("Flavor.3: Given a file expressed using Common flavor, When mdfmt -f Common, Then no change."),

        new TestCaseData("Flavor_azure.md", new string[] {"-f", "Azure"}, "Flavor_azure.md").
        SetName("Flavor.4: Given a file expressed using Azure flavor, When mdfmt -f Azure, Then no change."),

        new TestCaseData("Empty-Destination_before.md", new string[] {"-f", "Common"}, "Empty-Destination_after_common.md").
        SetName("Empty-Destination.1: Given a file with empty destinations, When mdfmt -f Common, Then destinations that can be matched to headings are filled in and the document is formatted according to the Common flavor."),

        new TestCaseData("Empty-Destination_before.md", new string[] {"-f", "Azure"}, "Empty-Destination_after_azure.md").
        SetName("Empty-Destination.2: Given a file with empty destinations, When mdfmt -f Azure, Then destinations that can be matched to headings are filled in and the document is formatted according to the Azure flavor."),

        new TestCaseData("Heading-Numbering_none.md", new string[] {"-h", "none"}, "Heading-Numbering_none.md").SetName("Heading-Numbering.1: Given a file without heading numbering, When heading numbering is removed, Then no change."),

        new TestCaseData("Heading-Numbering_none.md", new string[] { "-h", "1"}, "Heading-Numbering_1.md").SetName("Heading-Numbering.2: Given a file without heading numbering, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData("Heading-Numbering_none.md", new string[] { "-h", "1."}, "Heading-Numbering_1dot.md").SetName("Heading-Numbering.3: Given a file without heading numbering, When apply heading numbering with trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData("Heading-Numbering_1.md", new string[] { "-h", "none"}, "Heading-Numbering_none.md").SetName("Heading-Numbering.4: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData("Heading-Numbering_1.md", new string[] { "-h", "1"}, "Heading-Numbering_1.md").SetName("Heading-Numbering.5: Given a file with heading numbering without a trailing period, When apply heading numbering with no trailing period, Then no change."),

        new TestCaseData("Heading-Numbering_1.md", new string[] { "-h", "1."}, "Heading-Numbering_1dot.md").SetName("Heading-Numbering.6: Given a file with heading numbering without a trailing period, When apply heading numbering with a trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData("Heading-Numbering_1dot.md", new string[] { "-h", "none"}, "Heading-Numbering_none.md").SetName("Heading-Numbering.7: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData("Heading-Numbering_1dot.md", new string[] { "-h", "1"}, "Heading-Numbering_1.md").SetName("Heading-Numbering.8: Given a file with heading numbering that includes a trailing period, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData("Heading-Numbering_1dot.md", new string[] { "-h", "1."}, "Heading-Numbering_1dot.md").SetName("Heading-Numbering.9: Given a file with heading numbering that includes a trailing period, When apply heading numbering with a trailing period, Then no change."),
    ];

    [TestCaseSource(nameof(_testCases))]
    public void RunProgramTest(string testInputFile, string[] args, string expectedOutputFile)
    {
        // Arrange
        string testInputFilePath = Path.Combine(_markdownDir, testInputFile);
        string programFilePath = Path.Combine(_workDir, $"file.md");
        File.Copy(testInputFilePath, programFilePath, overwrite: true);
        List<string> argsToUse = new(args)
            {
                programFilePath
            };

        // Act
        int exitCode = Program.RunProgram([.. argsToUse], doExit: false);

        // Assert
        string actualFileContent = File.ReadAllText(programFilePath);
        string expectedFilePath = Path.Combine(_markdownDir, expectedOutputFile);
        string expectedFileContent = File.ReadAllText(expectedFilePath);
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(actualFileContent, Is.EqualTo(expectedFileContent));
        });
    }
}
