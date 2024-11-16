using Mdfmt.Loaders;
using Mdfmt.Model;
using System;
using System.Collections.Generic;

namespace Integration.Mdfmt.Loaders;

[TestFixture]
public class LineParserTests
{
    private LineParser _parser;

    [SetUp]
    public void Setup()
    {
        _parser = new LineParser();
    }

    private static string _s;

    private static readonly IEnumerable<TestCaseData> _lineParserTestCases =
    [
        new TestCaseData(false, false, "<!-- whole line is commented out -->", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("WholeLineCommentedOut"),
        new TestCaseData(true, true, "stuff", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("SubsequentLineStillCommented"),
        new TestCaseData(true, false, "-->after comment", Types(typeof(CommentRegion),typeof(ContentRegion)), true, "after comment").
            SetName("SubsequentLineImmediatelyEndsComment"),
        new TestCaseData(true, false, "still commented -->after comment", Types(typeof(CommentRegion),typeof(ContentRegion)), true, "after comment").
            SetName("SubsequentLineEndsComment"),
        new TestCaseData(false, true, "Hello<!-- unterminated comment", Types(typeof(ContentRegion), typeof(CommentRegion)), true, "Hello").
            SetName("UnterminatedComment1"),
        new TestCaseData(false, true, "<!-- unterminated comment", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("UnterminatedComment2"),
        new TestCaseData(false, false, string.Empty, Array.Empty<Type>(), false, string.Empty).
            SetName("EmptyLine"),
        new TestCaseData(true, true, string.Empty, Array.Empty<Type>(), false, string.Empty).
            SetName("EmptyCommentLine"),
        new TestCaseData(false, false, _s="[Example of a link!](https://www.example.com)", Types(typeof(LinkRegion)), true, _s).
            SetName("LineWithJustLink"),
        new TestCaseData(false, false, _s="[]()", Types(typeof(LinkRegion)), true, _s).
            SetName("TrivialLink"),
        new TestCaseData(false, false, _s="[label](destination)", Types(typeof(LinkRegion)), true, _s).
            SetName("AlmostLink1.LinkFound"),
        new TestCaseData(false, false, _s="[label](destination", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink1.Short1"),
        new TestCaseData(false, false, _s="[label](", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink1.Short2"),
        new TestCaseData(false, false, _s="[label]", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink1.Short3"),
        new TestCaseData(false, false, _s="[label", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink1.Short4"),
        new TestCaseData(false, false, _s="[", Types(typeof(ContentRegion)), true, "[").
            SetName("AlmostLink1.Short5"),
        new TestCaseData(false, false, _s="before [label](destination) after", Types(typeof(ContentRegion),typeof(LinkRegion),typeof(ContentRegion)), true, _s).
            SetName("AlmostLink2.LinkFound"),
        new TestCaseData(false, false, _s="before [label](destination after", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink2.Short1"),
        new TestCaseData(false, false, _s="before [label]( after", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink2.Short2"),
        new TestCaseData(false, false, _s="before [label] after", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink2.Short3"),
        new TestCaseData(false, false, _s="before [label after", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink2.Short4"),
        new TestCaseData(false, false, _s="before [ after", Types(typeof(ContentRegion)), true, _s).
            SetName("AlmostLink2.Short5"),
        new TestCaseData(false, false, _s="# Heading", Types(typeof(HeadingRegion)), true, _s).
            SetName("BasicHeading"),
        new TestCaseData(false, false, _s=" # Heading", Types(typeof(HeadingRegion)), true, _s).
            SetName("HeadingWithLeadingSpace"),
        new TestCaseData(false, false, "# Hea<!--comment in the middle-->ding", Types(typeof(HeadingRegion)), true, "# Heading").
            SetName("HeadingWithCommentInTheMiddle"),
        new TestCaseData(false, false, _s="My favorite opening comment:  `<!--`.  It is cool.", Types(typeof(ContentRegion)), true, _s).
            SetName("OpenHtmlCommentInBacktick"),
        new TestCaseData(false, false, _s="`", Types(typeof(ContentRegion)), true, _s).
            SetName("UnclosedBacktick1"),
        new TestCaseData(false, false, _s=" `", Types(typeof(ContentRegion)), true, _s).
            SetName("UnclosedBacktick2"),
        new TestCaseData(false, false, _s=" `abc", Types(typeof(ContentRegion)), true, _s).
            SetName("UnclosedBacktick3"),
        new TestCaseData(true, false, "-->## Eeked out a heading", Types(typeof(HeadingRegion)), true, "## Eeked out a heading").
            SetName("EekedOutHeading"),
        new TestCaseData(false, false, _s="-->## Not a heading", Types(typeof(ContentRegion)), true, _s).
            SetName("NotHeading"),
        new TestCaseData(false, false, _s=string.Empty, Array.Empty<Type>(), false, _s).
            SetName("Parser.01"),
        new TestCaseData(false, false, _s="<", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.02"),
        new TestCaseData(false, false, _s="<!", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.03"),
        new TestCaseData(false, false, _s="<!-", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.04"),
        new TestCaseData(false, true, "<!--", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("Parser.05"),
        new TestCaseData(false, true, "<!---", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("Parser.06"),
        new TestCaseData(false, true, "<!----", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("Parser.07"),
        new TestCaseData(false, false, "<!---->", Types(typeof(CommentRegion)), false, string.Empty).
            SetName("Parser.08"),
        new TestCaseData(false, false, _s="`", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.09"),
        new TestCaseData(false, false, _s="`x", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.10"),
        new TestCaseData(false, false, _s="`x`", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.11"),
        new TestCaseData(false, false, _s="`<!--`", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.12"),
        new TestCaseData(false, false, _s="`[link](destination)`", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.13"),
        new TestCaseData(false, false, _s="!", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.14"),
        new TestCaseData(false, false, _s="![", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.15"),
        new TestCaseData(false, false, _s="![label", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.16"),
        new TestCaseData(false, false, _s="![label]", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.17"),
        new TestCaseData(false, false, _s="![label](", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.18"),
        new TestCaseData(false, false, _s="![label](destination", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.19"),
        new TestCaseData(false, false, _s="![label](destination)", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.20"),
        new TestCaseData(false, false, _s="[", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.21"),
        new TestCaseData(false, false, _s="[label", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.22"),
        new TestCaseData(false, false, _s="[label]", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.23"),
        new TestCaseData(false, false, _s="[label](", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.24"),
        new TestCaseData(false, false, _s="[label](destination", Types(typeof(ContentRegion)), true, _s).
            SetName("Parser.25"),
        new TestCaseData(false, false, _s="[label](destination)", Types(typeof(LinkRegion)), true, _s).
            SetName("Parser.26"),
    ];

    /// <summary>
    /// Test the LineParser
    /// </summary>
    /// <param name="inHtmlComment">
    /// Whether in HTML comment when the parser is called.
    /// </param>
    /// <param name="expectedFinalInHtmlComment">
    /// Whether the parser's final state indicates in HTML comment
    /// </param>
    /// <param name="line">
    /// The line to parse.
    /// </param>
    /// <param name="expectedRegionTypes">
    /// An enumerable over the types expected to be seen in the region produced from the parse.
    /// </param>
    /// <param name="expectedIsActive">
    /// Whether it is expected that the parsed line has any active (uncommented) content.
    /// </param>
    /// <param name="expectedActiveContent">
    /// Expectation for the concatenation of all the content from active (uncommented) regions of
    /// the parse result.
    /// </param>
    [Test, TestCaseSource(nameof(_lineParserTestCases))]
    public void TestLineParser_Parse(bool inHtmlComment, bool expectedFinalInHtmlComment, string line, IEnumerable<Type> expectedRegionTypes, bool expectedIsActive, string expectedActiveContent)
    {
        List<Region> regions = _parser.Parse(line, inHtmlComment);

        Assert.Multiple(() =>
        {
            Assert.That(regions.Types, Is.EquivalentTo(expectedRegionTypes));
            Assert.That(regions.Content, Is.EqualTo(line));
            Assert.That(regions.IsActive, Is.EqualTo(expectedIsActive));
            Assert.That(regions.ActiveContent, Is.EqualTo(expectedActiveContent));
            Assert.That(regions.IsModified, Is.False);
            Assert.That(_parser.InHtmlComment, Is.EqualTo(expectedFinalInHtmlComment));
        });
    }

    private static Type[] Types(params Type[] types)
    {
        return types;
    }
}
