using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Mdfmt;

namespace Integration.Mdfmt;

[TestFixture]
public class ProgramTests
{
    private static readonly string _workDir = "./Work";
    private static readonly string _dataDir = "./Data";
    private static readonly string _programTestsZip = "ProgramTests.zip";
    private static readonly string _programTestsDir = "./Data/ProgramTests";
    private static readonly string _empty_md = "Empty.md";
    private static readonly string _title_md = "Title.md";
    private static readonly string _title_toc_md = "Title_toc.md";
    private static readonly string _title_tocAzure_updated_md = "Title_toc-azure_updated.md";
    private static readonly string _title_tocAzure_updated2_md = "Title_toc-azure_updated2.md";
    private static readonly string _title_tocCommon_outdated_md = "Title_toc-common_outdated.md";
    private static readonly string _title_tocCommon_updated_md = "Title_toc-common_updated.md";
    private static readonly string _title_tocCommon_updated2_md = "Title_toc-common_updated2.md";
    private static readonly string _lineEndings_unix_md = "Line-Endings_unix.md";
    private static readonly string _lineEndings_windows_md = "Line-Endings_windows.md";
    private static readonly string _lineEndings_mixed_md = "Line-Endings_mixed.md";
    private static readonly string _lineEndings_mixed_fixed_for_unix_md = "Line-Endings_mixed_fixed_for_unix.md";
    private static readonly string _lineEndings_mixed_fixed_for_windows_md = "Line-Endings_mixed_fixed_for_windows.md";
    private static readonly string _flavor_azure_md = "Flavor_azure.md";
    private static readonly string _flavor_common_md = "Flavor_common.md";
    private static readonly string _emptyDestination_before_md = "Empty-Destination_before.md";
    private static readonly string _emptyDestination_after_common_md = "Empty-Destination_after_common.md";
    private static readonly string _emptyDestination_after_azure_md = "Empty-Destination_after_azure.md";
    private static readonly string _headingNumbering_none_md = "Heading-Numbering_none.md";
    private static readonly string _headingNumbering_1_md = "Heading-Numbering_1.md";
    private static readonly string _headingNumbering_1dot_md = "Heading-Numbering_1dot.md";
    private static readonly string _NewlinesThenContent_unix_md = "Newlines-Then-Content_unix.md";
    private static readonly string _NewlinesThenContent_windows_md = "Newlines-Then-Content_windows.md";
    private static readonly string _FiveNewlines_unix_md = "Five-Newlines_unix.md";
    private static readonly string _FiveNewlines_windows_md = "Five-Newlines_windows.md";
    private static readonly string _UnclosedFencedRegion_unix_md = "Unclosed-Fenced-Region_unix.md";
    private static readonly string _UnclosedFencedRegion_windows_md = "Unclosed-Fenced-Region_windows.md";
    private static readonly string _UnclosedTocRegion_windows_after_md = "Unclosed-Toc-Region_windows_after.md";
    private static readonly string _UnclosedTocRegion_windows_before_md = "Unclosed-Toc-Region_windows_before.md";
    private static readonly string _UnclosedTocRegion_unix_after_md = "Unclosed-Toc-Region_unix_after.md";
    private static readonly string _UnclosedTocRegion_unix_before_md = "Unclosed-Toc-Region_unix_before.md";
    private static readonly string _addingToc_mainly_windows_before_md = "Adding-Toc_mainly_windows_before.md";
    private static readonly string _addingToc_mainly_windows_after_md = "Adding-Toc_mainly_windows_after.md";
    private static readonly string _addingToc_mainly_unix_before_md = "Adding-Toc_mainly_unix_before.md";
    private static readonly string _addingToc_mainly_unix_after_md = "Adding-Toc_mainly_unix_after.md";
    private static readonly string _addingToc_all_windows_after_md = "Adding-Toc_all_windows_after.md";
    private static readonly string _addingToc_all_unix_after_md = "Adding-Toc_all_unix_after.md";
    private static readonly string _fencedExamples_l0_md = "Fenced-Examples_l0.md";
    private static readonly string _fencedExamples_l1_md = "Fenced-Examples_l1.md";
    private static readonly string _fenced_l0_md = "Fenced_l0.md";
    private static readonly string _fenced_l1_md = "Fenced_l1.md";
    private static readonly string _fenced_l2_md = "Fenced_l2.md";
    private static readonly string _fenced_l3_md = "Fenced_l3.md";


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Directory.CreateDirectory(_workDir);

        if (!Directory.Exists(_programTestsDir))
        {
            ZipFile.ExtractToDirectory(Path.Combine(_dataDir, _programTestsZip), _dataDir);
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Directory.Delete(_programTestsDir, true);
        Directory.Delete(_workDir, true);
    }

    [SetUp]
    public void Setup()
    {
    }

    private static readonly IEnumerable<TestCaseData> _testCases =
    [
        new TestCaseData(_empty_md, Array.Empty<string>(), _empty_md, ExitCodes.Success).
        SetName("Empty.1: Given an empty file, When no arguments, Then empty file."),

        new TestCaseData(_empty_md, new string[] {"-f", "azure", "-h", "1", "-t", "2", "--newline-strategy", "PreferWindows", "-v"}, _empty_md, ExitCodes.Success).
        SetName("Empty.2: Given an empty file, When several arguments, Then empty file."),



        new TestCaseData(_title_md, new string[] {"-t", "1", "-f", "common"}, _title_toc_md, ExitCodes.Success).
        SetName("TOC.1a: Given a file with a title only, add a common flavored TOC."),

        new TestCaseData(_title_md, new string[] {"-t", "1", "-f", "azure"}, _title_toc_md, ExitCodes.Success).
        SetName("TOC.1b: Given a file with a title only, add an Azure flavored TOC."),

        new TestCaseData(_title_md, new string[] {"--toc-threshold", "1", "--flavor", "Common"}, _title_toc_md, ExitCodes.Success).
        SetName("TOC.2: Given a file with a title only, When mdfmt --toc-threshold 1, Then a TOC is added."),

        new TestCaseData(_title_md, new string[] {"-t", "0"}, _title_md, ExitCodes.Success).
        SetName("TOC.3: Given a file with a title only, When mdfmt -t 0, Then no change."),

        new TestCaseData(_title_toc_md, new string[] {"-t", "0"}, _title_md, ExitCodes.Success).
        SetName("TOC.4: Given a file with a title and TOC, When mdfmt -t 0, Then the TOC is removed."),

        new TestCaseData(_title_toc_md, new string[] {"--toc-threshold", "0"}, _title_md, ExitCodes.Success).
        SetName("TOC.5: Given a file with a title and TOC, When mdfmt --toc-threshold 0, Then the TOC is removed."),

        new TestCaseData(_title_toc_md, new string[] {"-t", "1", "-f", "common"}, _title_toc_md, ExitCodes.Success).
        SetName("TOC.6a: Given a file with a title and TOC, when re-adding the TOC, Then no change."),

        new TestCaseData(_title_toc_md, new string[] {"-t", "1", "-f", "azure"}, _title_toc_md, ExitCodes.Success).
        SetName("TOC.6b: Given a file with a title and TOC, when re-adding the TOC, Then no change."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-f", "common"}, _title_tocCommon_updated_md, ExitCodes.Success).
        SetName("TOC.7a: Given a file with outdated common TOC and headings, update the TOC using Common flavor."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-t", "1", "-f", "common"}, _title_tocCommon_updated_md, ExitCodes.Success).
        SetName("TOC.7b: Given a file with outdated common TOC and headings, update the TOC using Common flavor."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-f", "azure"}, _title_tocAzure_updated_md, ExitCodes.Success).
        SetName("TOC.7c: Given a file with outdated common TOC and headings, update the TOC using Azure flavor."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-t", "1", "-f", "azure"}, _title_tocAzure_updated_md, ExitCodes.Success).
        SetName("TOC.7d: Given a file with outdated common TOC and headings, update the TOC using Azure flavor."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-h", "1.", "-f", "common"}, _title_tocCommon_updated2_md, ExitCodes.Success).
        SetName("TOC.8a: Given a file with outdated common TOC and headings, update both the TOC using Common flavor and headings."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-h", "1.", "-t", "1", "-f", "common"}, _title_tocCommon_updated2_md, ExitCodes.Success).
        SetName("TOC.8b: Given a file with outdated common TOC and headings, update both the TOC using Common flavor and headings."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-h", "1.", "-f", "azure"}, _title_tocAzure_updated2_md, ExitCodes.Success).
        SetName("TOC.8c: Given a file with outdated common TOC and headings, update the TOC using Azure flavor and headings."),

        new TestCaseData(_title_tocCommon_outdated_md, new string[] {"-h", "1.", "-t", "1", "-f", "azure"}, _title_tocAzure_updated2_md, ExitCodes.Success).
        SetName("TOC.8d: Given a file with outdated common TOC and headings, update the TOC using Azure flavor and headings."),

        new TestCaseData(_addingToc_mainly_windows_before_md, new string[] {"-t", "1", "-f", "common"}, _addingToc_mainly_windows_after_md, ExitCodes.Success).
        SetName("TOC.9: Given a doc with mostly windows line endings, when inserting TOC, windows line endings are used by default"),

        new TestCaseData(_addingToc_mainly_unix_before_md, new string[] {"-t", "1", "-f", "common"}, _addingToc_mainly_unix_after_md, ExitCodes.Success).
        SetName("TOC.10: Given a doc with mostly unix line endings, when inserting TOC, unix line endings are used by default"),

        new TestCaseData(_addingToc_mainly_windows_before_md, new string[] {"-t", "1", "-f", "common", "--newline-strategy", "unix"}, _addingToc_all_unix_after_md, ExitCodes.Success).
        SetName("TOC.11: Given a doc with mostly windows line endings, when inserting TOC with unix newline strategy, results in a file with a TOC and only unix line endings"),

        new TestCaseData(_addingToc_mainly_unix_before_md, new string[] {"-t", "1", "-f", "common", "--newline-strategy", "windows"}, _addingToc_all_windows_after_md, ExitCodes.Success).
        SetName("TOC.12: Given a doc with mostly unix line endings, when inserting TOC with windows newline strategy, results in a file with a TOC and only windows line endings"),



        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "Unix"}, _lineEndings_unix_md, ExitCodes.Success).
        SetName("Line-Endings.01: Given a file with unix line endings, When mdfmt --newline-strategy Unix, Then no change."),

        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "Windows"}, _lineEndings_windows_md, ExitCodes.Success).
        SetName("Line-Endings.02: Given a file with unix line endings, When mdfmt --newline-strategy Windows, Then Windows line endings."),

        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "PreferUnix"}, _lineEndings_unix_md, ExitCodes.Success).
        SetName("Line-Endings.03: Given a file with unix line endings, When mdfmt --newline-strategy PreferUnix, Then no change."),

        new TestCaseData(_lineEndings_unix_md, new string[] {"--newline-strategy", "PreferWindows"}, _lineEndings_unix_md, ExitCodes.Success).
        SetName("Line-Endings.04: Given a file with unix line endings, When mdfmt --newline-strategy PreferWindows, Then no change."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "Unix"}, _lineEndings_unix_md, ExitCodes.Success).
        SetName("Line-Endings.05: Given a file with windows line endings, When mdfmt --newline-strategy Unix, Then Unix line endings."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "Windows"}, _lineEndings_windows_md, ExitCodes.Success).
        SetName("Line-Endings.06: Given a file with windows line endings, When mdfmt --newline-strategy Windows, Then no change."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "PreferUnix"}, _lineEndings_windows_md, ExitCodes.Success).
        SetName("Line-Endings.07: Given a file with windows line endings, When mdfmt --newline-strategy PreferUnix, Then no change."),

        new TestCaseData(_lineEndings_windows_md, new string[] {"--newline-strategy", "PreferWindows"}, _lineEndings_windows_md, ExitCodes.Success).
        SetName("Line-Endings.08: Given a file with windows line endings, When mdfmt --newline-strategy PreferWindows, Then no change."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "Unix"}, _lineEndings_mixed_fixed_for_unix_md, ExitCodes.Success).
        SetName("Line-Endings.09: Given a file with mixed line endings, When mdfmt --newline-strategy Unix, Then Unix line endings."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "Windows"}, _lineEndings_mixed_fixed_for_windows_md, ExitCodes.Success).
        SetName("Line-Endings.10: Given a file with mixed line endings, When mdfmt --newline-strategy Windows, Then Windows line endings."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "PreferUnix"}, _lineEndings_mixed_fixed_for_unix_md, ExitCodes.Success).
        SetName("Line-Endings.11: Given a file with mixed line endings, When mdfmt --newline-strategy PreferUnix, Then Unix line endings."),

        new TestCaseData(_lineEndings_mixed_md, new string[] {"--newline-strategy", "PreferWindows"}, _lineEndings_mixed_fixed_for_windows_md, ExitCodes.Success).
        SetName("Line-Endings.12: Given a file with mixed line endings, When mdfmt --newline-strategy PreferWindows, Then Windows line endings."),

        new TestCaseData(_NewlinesThenContent_unix_md, new string[] {"--newline-strategy", "Windows"}, _NewlinesThenContent_windows_md, ExitCodes.Success).
        SetName("Line-Endings.13: Preserve leading newlines when converting from Unix to Windows newlines"),

        new TestCaseData(_NewlinesThenContent_windows_md, new string[] {"--newline-strategy", "Unix"}, _NewlinesThenContent_unix_md, ExitCodes.Success).
        SetName("Line-Endings.14: Preserve leading newlines when converting from Windows to Unix newlines"),

        new TestCaseData(_FiveNewlines_unix_md, new string[] {"--newline-strategy", "Windows"}, _FiveNewlines_windows_md, ExitCodes.Success).
        SetName("Line-Endings.15: Convert a file of only newlines from Unix to Windows"),

        new TestCaseData(_FiveNewlines_windows_md, new string[] {"--newline-strategy", "Unix"}, _FiveNewlines_unix_md, ExitCodes.Success).
        SetName("Line-Endings.16: Convert a file of only newlines from Windows to Unix"),



        new TestCaseData(_UnclosedFencedRegion_unix_md, new string[] {"--newline-strategy", "Windows"}, _UnclosedFencedRegion_windows_md, ExitCodes.Success).
        SetName("Line-Endings-Unclosed.1: Given a file that ends with an unclosed fenced code block, converting from unix to windows does not lose information."),

        new TestCaseData(_UnclosedFencedRegion_windows_md, new string[] {"--newline-strategy", "Unix"}, _UnclosedFencedRegion_unix_md, ExitCodes.Success).
        SetName("Line-Endings-Unclosed.2: Given a file that ends with an unclosed fenced code block, converting from windows to unix does not lose information."),

        new TestCaseData(_UnclosedTocRegion_unix_before_md, new string[] {"--newline-strategy", "Windows"}, _UnclosedTocRegion_windows_after_md, ExitCodes.Success).
        SetName("Line-Endings-Unclosed.3: Given a file that ends with an unclosed TOC, converting from unix to windows closes TOC and changes newlines."),

        new TestCaseData(_UnclosedTocRegion_windows_before_md, new string[] {"--newline-strategy", "Unix"}, _UnclosedTocRegion_unix_after_md, ExitCodes.Success).
        SetName("Line-Endings-Unclosed.4: Given a file that ends with an unclosed TOC, converting from windows to unix closes TOC and changes newlines."),



        new TestCaseData(_flavor_common_md, new string[] {"-f", "Azure"}, _flavor_azure_md, ExitCodes.Success).
        SetName("Flavor.1: Given a file expressed using Common flavor, When mdfmt -f Azure, Then the file is converted to Azure flavor."),

        new TestCaseData(_flavor_azure_md, new string[] {"-f", "Common"}, _flavor_common_md, ExitCodes.Success).
        SetName("Flavor.2: Given a file expressed using Azure flavor, When mdfmt -f Common, Then the file is converted to Common flavor."),

        new TestCaseData(_flavor_common_md, new string[] {"-f", "Common"}, _flavor_common_md, ExitCodes.Success).
        SetName("Flavor.3: Given a file expressed using Common flavor, When mdfmt -f Common, Then no change."),

        new TestCaseData(_flavor_azure_md, new string[] {"-f", "Azure"}, _flavor_azure_md, ExitCodes.Success).
        SetName("Flavor.4: Given a file expressed using Azure flavor, When mdfmt -f Azure, Then no change."),



        new TestCaseData(_emptyDestination_before_md, new string[] {"-f", "Common"}, _emptyDestination_after_common_md, ExitCodes.Success).
        SetName("Empty-Destination.1: Given a file with empty destinations, When mdfmt -f Common, Then destinations that can be matched to headings are filled in and the document is formatted according to the Common flavor."),

        new TestCaseData(_emptyDestination_before_md, new string[] {"-f", "Azure"}, _emptyDestination_after_azure_md, ExitCodes.Success).
        SetName("Empty-Destination.2: Given a file with empty destinations, When mdfmt -f Azure, Then destinations that can be matched to headings are filled in and the document is formatted according to the Azure flavor."),



        new TestCaseData(_headingNumbering_none_md, new string[] {"-h", "none", "-f", "Common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.1a: Given a file without heading numbering, When heading numbering is removed, Then no change."),

        new TestCaseData(_headingNumbering_none_md, new string[] {"--heading-numbers", "NONE", "--flavor", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.1b: Given a file without heading numbering, When heading numbering is removed, Then no change."),

        new TestCaseData(_headingNumbering_none_md, new string[] {"-h", "1", "-t", "1", "-f", "common"}, _headingNumbering_1_md, ExitCodes.Success).
        SetName("Heading-Numbering.2a: Given a file without heading numbering, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData(_headingNumbering_none_md, new string[] {"-h", "1", "-f", "common"}, _headingNumbering_1_md, ExitCodes.Success).
        SetName("Heading-Numbering.2b: Given a file without heading numbering, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData(_headingNumbering_none_md, new string[] {"-h", "1.", "-t", "1", "-f", "common"}, _headingNumbering_1dot_md, ExitCodes.Success).
        SetName("Heading-Numbering.3a: Given a file without heading numbering, When apply heading numbering with trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData(_headingNumbering_none_md, new string[] {"-h", "1.", "-f", "common"}, _headingNumbering_1dot_md, ExitCodes.Success).
        SetName("Heading-Numbering.3b: Given a file without heading numbering, When apply heading numbering with trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"-h", "none", "-t", "1", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.4a: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"--heading-numbers", "NONE", "-t", "1", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.4b: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"-h", "none", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.4c: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"--heading-numbers", "NONE", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.4d: Given a file with heading numbering without a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"-h", "1", "-f", "common"}, _headingNumbering_1_md, ExitCodes.Success).
        SetName("Heading-Numbering.5: Given a file with heading numbering without a trailing period, When apply heading numbering with no trailing period, Then no change."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"-h", "1.", "-t", "1", "-f", "common"}, _headingNumbering_1dot_md, ExitCodes.Success).
        SetName("Heading-Numbering.6a: Given a file with heading numbering without a trailing period, When apply heading numbering with a trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData(_headingNumbering_1_md, new string[] {"-h", "1.", "-f", "common"}, _headingNumbering_1dot_md, ExitCodes.Success).
        SetName("Heading-Numbering.6b: Given a file with heading numbering without a trailing period, When apply heading numbering with a trailing period, Then heading numbering with trailing period is applied."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] {"-h", "none", "-t", "1", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.7a: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] {"--heading-numbers", "NONE", "-t", "1", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.7b: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] {"-h", "none", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.7c: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] {"--heading-numbers", "NONE", "-f", "common"}, _headingNumbering_none_md, ExitCodes.Success).
        SetName("Heading-Numbering.7d: Given a file with heading numbering that includes a trailing period, When heading numbering is removed, Then there are no heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] {"-h", "1", "-f", "common"}, _headingNumbering_1_md, ExitCodes.Success).
        SetName("Heading-Numbering.8: Given a file with heading numbering that includes a trailing period, When apply heading numbering with no trailing period, Then heading numbering with no trailing period is applied."),

        new TestCaseData(_headingNumbering_1dot_md, new string[] {"-h", "1.", "-f", "common"}, _headingNumbering_1dot_md, ExitCodes.Success).
        SetName("Heading-Numbering.9: Given a file with heading numbering that includes a trailing period, When apply heading numbering with a trailing period, Then no change."),

        new TestCaseData(_headingNumbering_1_md, Array.Empty<string>(), _headingNumbering_1_md, ExitCodes.Success).
        SetName("Heading-Numbering.10: By default, do not change heading numbers."),

        new TestCaseData(_headingNumbering_1dot_md, Array.Empty<string>(), _headingNumbering_1dot_md, ExitCodes.Success).
        SetName("Heading-Numbering.11: By default, do not change heading numbers."),



        new TestCaseData(_fencedExamples_l0_md, new string[] {"-l", "1"}, _fencedExamples_l1_md, ExitCodes.Success).SetName("Fenced.1a"),

        new TestCaseData(_fencedExamples_l0_md, new string[] {"--line-numbering-threshold", "1"}, _fencedExamples_l1_md, ExitCodes.Success).SetName("Fenced.1b"),

        new TestCaseData(_fenced_l0_md, new string[] {"-l", "0"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.0-->0"),

        new TestCaseData(_fenced_l0_md, new string[] {"-l", "1"}, _fenced_l1_md, ExitCodes.Success).SetName("Fenced.0-->1"),

        new TestCaseData(_fenced_l0_md, new string[] {"-l", "2"}, _fenced_l2_md, ExitCodes.Success).SetName("Fenced.0-->2"),

        new TestCaseData(_fenced_l0_md, new string[] {"-l", "3"}, _fenced_l3_md, ExitCodes.Success).SetName("Fenced.0-->3"),

        new TestCaseData(_fenced_l0_md, new string[] {"-l", "4"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.0-->4"),

        new TestCaseData(_fenced_l1_md, new string[] {"-l", "0"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.1-->0"),

        new TestCaseData(_fenced_l1_md, new string[] {"-l", "1"}, _fenced_l1_md, ExitCodes.Success).SetName("Fenced.1-->1"),

        new TestCaseData(_fenced_l1_md, new string[] {"-l", "2"}, _fenced_l2_md, ExitCodes.Success).SetName("Fenced.1-->2"),

        new TestCaseData(_fenced_l1_md, new string[] {"-l", "3"}, _fenced_l3_md, ExitCodes.Success).SetName("Fenced.1-->3"),

        new TestCaseData(_fenced_l1_md, new string[] {"-l", "4"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.1-->4"),

        new TestCaseData(_fenced_l2_md, new string[] {"-l", "0"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.2-->0"),

        new TestCaseData(_fenced_l2_md, new string[] {"-l", "1"}, _fenced_l1_md, ExitCodes.Success).SetName("Fenced.2-->1"),

        new TestCaseData(_fenced_l2_md, new string[] {"-l", "2"}, _fenced_l2_md, ExitCodes.Success).SetName("Fenced.2-->2"),

        new TestCaseData(_fenced_l2_md, new string[] {"-l", "3"}, _fenced_l3_md, ExitCodes.Success).SetName("Fenced.2-->3"),

        new TestCaseData(_fenced_l2_md, new string[] {"-l", "4"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.2-->4"),

        new TestCaseData(_fenced_l3_md, new string[] {"-l", "0"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.3-->0"),

        new TestCaseData(_fenced_l3_md, new string[] {"-l", "1"}, _fenced_l1_md, ExitCodes.Success).SetName("Fenced.3-->1"),

        new TestCaseData(_fenced_l3_md, new string[] {"-l", "2"}, _fenced_l2_md, ExitCodes.Success).SetName("Fenced.3-->2"),

        new TestCaseData(_fenced_l3_md, new string[] {"-l", "3"}, _fenced_l3_md, ExitCodes.Success).SetName("Fenced.3-->3"),

        new TestCaseData(_fenced_l3_md, new string[] {"-l", "4"}, _fenced_l0_md, ExitCodes.Success).SetName("Fenced.3-->4"),



        new TestCaseData(null, new string[] { "--bogus-option" }, null, ExitCodes.MisuseOfCommand).
        SetName("Options.01: When the program is run with unknown options, Then an exit code of MisueOfCommand."),

        new TestCaseData(null, new string[] { "--help" }, null, ExitCodes.Success).
        SetName("Options.02: When the program is run with the --help option, Then an exit code of Success."),

        new TestCaseData(null, new string[] { "--version" }, null, ExitCodes.Success).
        SetName("Options.03: When the program is run with the --version option, Then an exit code of Success."),

        new TestCaseData(null, new string[] {"-h", "bad-value"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.04: When the program is run with a bad value passed to the -h option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"--heading-numbers", "bad-value"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.05: When the program is run with a bad value passed to the --heading-numbers option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"-f", "bad-value"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.06: When the program is run with a bad value passed to the -f option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"--flavor", "bad-value"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.07: When the program is run with a bad value passed to the --flavor option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"-t", "-1"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.08: When the program is run with a negative value passed to the -t option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"--toc-threshold", "-1"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.09: When the program is run with a negative value passed to the --toc-threshold option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"--newline-strategy", "bad-value"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.10: When the program is run with a bad value passed to the --newline-strategy option, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"-l", "-1"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.11: When the program is run with a bad value passed to the -l, Then an exit code of MisuseOfCommand."),

        new TestCaseData(null, new string[] {"--line-numbering-threshold", "-1"}, null, ExitCodes.MisuseOfCommand).
        SetName("Options.12: When the program is run with a bad value passed to the --line-numbering-threshold, Then an exit code of MisuseOfCommand."),
    ];

    [TestCaseSource(nameof(_testCases))]
    public void RunProgramTest(string testInputFile, IEnumerable<string> args, string expectedOutputFile, int expectedExitCode)
    {
        bool filesProvided = testInputFile != null;

        // Arrange
        List<string> argsToUse = new(args);

        string filePath = null;
        if (filesProvided)
        {
            string testInputFilePath = Path.Combine(_programTestsDir, testInputFile);
            filePath = Path.Combine(_workDir, $"file.md");
            File.Copy(testInputFilePath, filePath, overwrite: true);
            argsToUse.Add(filePath);
        }

        // Act
        int exitCode = Program.RunProgram([.. argsToUse], doExit: false);

        // Assert

        Assert.That(exitCode, Is.EqualTo(expectedExitCode));
        if (filesProvided)
        {
            string actualFileContent = File.ReadAllText(filePath);
            string expectedFilePath = Path.Combine(_programTestsDir, expectedOutputFile);
            string expectedFileContent = File.ReadAllText(expectedFilePath);
            Assert.That(actualFileContent, Is.EqualTo(expectedFileContent));
        }
    }
}
