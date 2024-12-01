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
    private static readonly string _markdownDir = "./Data/ProgramTests";
    private static readonly string _empty_md = "Empty.md";
    private static readonly string _title_md = "Title.md";
    private static readonly string _title_toc_md = "Title_toc.md";
    private static readonly string _title_toc_outdated_md = "Title_toc_outdated.md";
    private static readonly string _title_toc_updated_md = "Title_toc_updated.md";
    private static readonly string _lineEndings_unix_md = "Line-Endings_unix.md";
    private static readonly string _lineEndings_windows_md = "Line-Endings_windows.md";
    private static readonly string _lineEndings_mixed_md = "Line-Endings_mixed.md";
    private static readonly string _lineEndings_mixed_fixed_for_unix_md = "Line-Endings_mixed_fixed_for_unix.md";
    private static readonly string _lineEndings_mixed_fixed_for_windows_md = "Line-Endings_mixed_fixed_for_windows.md";
    private static readonly string _flavor_common_md = "Flavor_common.md";
    private static readonly string _flavor_azure_md = "Flavor_azure.md";
    private static readonly string _emptyDestination_before_md = "Empty-Destination_before.md";
    private static readonly string _emptyDestination_after_common_md = "Empty-Destination_after_common.md";
    private static readonly string _emptyDestination_after_azure_md = "Empty-Destination_after_azure.md";
    private static readonly string _headingNumbering_none_md = "Heading-Numbering_none.md";
    private static readonly string _headingNumbering_1_md = "Heading-Numbering_1.md";
    private static readonly string _headingNumbering_1dot_md = "Heading-Numbering_1dot.md";

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
        new TestCaseData(_empty_md, Array.Empty<string>(), _empty_md).
        SetName("Empty.1: Given an empty file, When no arguments, Then empty file."),

        new TestCaseData(_empty_md, new string[] {"-f", "azure", "-h", "1", "-t", "2", "--newline-strategy", "PreferWindows", "-v"}, _empty_md).
        SetName("Empty.2: Given an empty file, When several arguments, Then empty file."),



        new TestCaseData(_title_md, new string[] {"-t", "1"}, _title_toc_md).
        SetName("TOC.1: Given a file with a title only, When mdfmt -t 1, Then a TOC is added."),

        new TestCaseData(_title_md, new string[] {"--toc-threshold", "1"}, _title_toc_md).
        SetName("TOC.2: Given a file with a title only, When mdfmt --toc-threshold 1, Then a TOC is added."),

        new TestCaseData(_title_md, new string[] {"-t", "0"}, _title_md).
        SetName("TOC.3: Given a file with a title only, When mdfmt -t 0, Then no change."),

        new TestCaseData(_title_toc_md, new string[] {"-t", "0"}, _title_md).
        SetName("TOC.4: Given a file with a title and TOC, When mdfmt -t 0, Then the TOC is removed."),

        new TestCaseData(_title_toc_md, new string[] {"--toc-threshold", "0"}, _title_md).
        SetName("TOC.5: Given a file with a title and TOC, When mdfmt --toc-threshold 0, Then the TOC is removed."),

        new TestCaseData(_title_toc_md, new string[] {"-t", "1"}, _title_toc_md).
        SetName("TOC.6: Given a file with a title and TOC, When mdfmt -t 1, Then no change."),

        new TestCaseData(_title_toc_outdated_md, new string[] {"-t", "1"}, _title_toc_updated_md).
        SetName("TOC.7: Given a file with an outdated TOC, When mdfmt -t 1, Then the TOC is updated."),



        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "Unix"}, _lineEndings_unix_md).
        SetName("Line-Endings.01: Given a file with unix line endings, When mdfmt --newline-strategy Unix, Then no change."),

        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "Windows"}, _lineEndings_windows_md).
        SetName("Line-Endings.02: Given a file with unix line endings, When mdfmt --newline-strategy Windows, Then Windows line endings."),

        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "PreferUnix"}, _lineEndings_unix_md).
        SetName("Line-Endings.03: Given a file with unix line endings, When mdfmt --newline-strategy PreferUnix, Then no change."),

        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "PreferWindows"}, _lineEndings_unix_md).
        SetName("Line-Endings.04: Given a file with unix line endings, When mdfmt --newline-strategy PreferWindows, Then no change."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "Unix"}, _lineEndings_unix_md).
        SetName("Line-Endings.05: Given a file with windows line endings, When mdfmt --newline-strategy Unix, Then Unix line endings."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "Windows"}, _lineEndings_windows_md).
        SetName("Line-Endings.06: Given a file with windows line endings, When mdfmt --newline-strategy Windows, Then no change."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "PreferUnix"}, _lineEndings_windows_md).
        SetName("Line-Endings.07: Given a file with windows line endings, When mdfmt --newline-strategy PreferUnix, Then no change."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "PreferWindows"}, _lineEndings_windows_md).
        SetName("Line-Endings.08: Given a file with windows line endings, When mdfmt --newline-strategy PreferWindows, Then no change."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "Unix"}, _lineEndings_mixed_fixed_for_unix_md).
        SetName("Line-Endings.09: Given a file with mixed line endings, When mdfmt --newline-strategy Unix, Then Unix line endings."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "Windows"}, _lineEndings_mixed_fixed_for_windows_md).
        SetName("Line-Endings.10: Given a file with mixed line endings, When mdfmt --newline-strategy Windows, Then Windows line endings."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "PreferUnix"}, _lineEndings_mixed_fixed_for_unix_md).
        SetName("Line-Endings.11: Given a file with mixed line endings, When mdfmt --newline-strategy PreferUnix, Then Unix line endings."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "PreferWindows"}, _lineEndings_mixed_fixed_for_windows_md).
        SetName("Line-Endings.12: Given a file with mixed line endings, When mdfmt --newline-strategy PreferWindows, Then Windows line endings."),



        new TestCaseData(_flavor_common_md, new string[] {"-f", "Azure"}, _flavor_azure_md).
        SetName("Flavor.1: Given a file expressed using Common flavor, When mdfmt -f Azure, Then the file is converted to Azure flavor."),

        new TestCaseData(_flavor_azure_md, new string[] {"-f", "Common"}, _flavor_common_md).
        SetName("Flavor.2: Given a file expressed using Azure flavor, When mdfmt -f Common, Then the file is converted to Common flavor."),

        new TestCaseData(_flavor_common_md, new string[] {"-f", "Common"}, _flavor_common_md).
        SetName("Flavor.3: Given a file expressed using Common flavor, When mdfmt -f Common, Then no change."),

        new TestCaseData(_flavor_azure_md, new string[] {"-f", "Azure"}, _flavor_azure_md).
        SetName("Flavor.4: Given a file expressed using Azure flavor, When mdfmt -f Azure, Then no change."),



        new TestCaseData(_emptyDestination_before_md, new string[] {"-f", "Common"}, _emptyDestination_after_common_md).
        SetName("Empty-Destination.1: Given a file with empty destinations, When mdfmt -f Common, Then destinations that can be matched to headings are filled in and the document is formatted according to the Common flavor."),

        new TestCaseData(_emptyDestination_before_md, new string[] {"-f", "Azure"}, _emptyDestination_after_azure_md).
        SetName("Empty-Destination.2: Given a file with empty destinations, When mdfmt -f Azure, Then destinations that can be matched to headings are filled in and the document is formatted according to the Azure flavor."),



        new TestCaseData(_headingNumbering_none_md, new string[] {"-h", "none"}, _headingNumbering_none_md).SetName("Heading-Numbering.1a: Given a file without heading numbering, When heading numbering is removed, Then no change."),

        new TestCaseData(_headingNumbering_none_md, new string[] {"--heading-numbers", "NONE"}, _headingNumbering_none_md).SetName("Heading-Numbering.1b: Given a file without heading numbering, When heading numbering is removed, Then no change."),

        new TestCaseData(_headingNumbering_none_md, new string[] { "-h", "1"}, _headingNumbering_1_md).SetName("Heading-Numbering.2: Given a file without heading numbering, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData(_headingNumbering_none_md, new string[] { "-h", "1."}, _headingNumbering_1dot_md).SetName("Heading-Numbering.3: Given a file without heading numbering, When apply heading numbering with trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData(_headingNumbering_1_md, new string[] { "-h", "none"}, _headingNumbering_none_md).SetName("Heading-Numbering.4a: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1_md, new string[] { "--heading-numbers", "NONE"}, _headingNumbering_none_md).SetName("Heading-Numbering.4b: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1_md, new string[] { "-h", "1"}, _headingNumbering_1_md).SetName("Heading-Numbering.5: Given a file with heading numbering without a trailing period, When apply heading numbering with no trailing period, Then no change."),

        new TestCaseData(_headingNumbering_1_md, new string[] { "-h", "1."}, _headingNumbering_1dot_md).SetName("Heading-Numbering.6: Given a file with heading numbering without a trailing period, When apply heading numbering with a trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] { "-h", "none"}, _headingNumbering_none_md).SetName("Heading-Numbering.7a: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] { "--heading-numbers", "NONE"}, _headingNumbering_none_md).SetName("Heading-Numbering.7b: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] { "-h", "1"}, _headingNumbering_1_md).SetName("Heading-Numbering.8: Given a file with heading numbering that includes a trailing period, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] { "-h", "1."}, _headingNumbering_1dot_md).SetName("Heading-Numbering.9: Given a file with heading numbering that includes a trailing period, When apply heading numbering with a trailing period, Then no change."),
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
