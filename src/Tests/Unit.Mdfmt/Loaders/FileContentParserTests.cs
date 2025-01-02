using Mdfmt.Loaders;
using System.Collections.Generic;

namespace Unit.Mdfmt.Loaders;

[TestFixture]
public class FileContentParserTests
{
    private static readonly IEnumerable<TestCaseData> _testCases =
    [
        new TestCaseData("", System.Array.Empty<string>()),
        new TestCaseData("\n", new string[] { "\n" }),
        new TestCaseData("\r", new string[] { "\r" }),
        new TestCaseData("\r\n", new string[] { "\r\n" }),
        new TestCaseData("\n\n\n", new string[] { "\n","\n","\n" }),
        new TestCaseData("\r\n\r\n\r\n", new string[] { "\r\n","\r\n","\r\n" }),
        new TestCaseData("\r\r\n\r\n\n\r\n\r\r\n\n\r", new string[] { "\r\r","\n","\r\n","\n","\r\n","\r\r","\n","\n","\r" }),
        new TestCaseData("hello", new string[] { "hello" }),
        new TestCaseData("hello\n", new string[] { "hello", "\n" }),
        new TestCaseData("hello\r", new string[] { "hello", "\r" }),
        new TestCaseData("line1\nline2", new string[] { "line1", "\n", "line2" }),
        new TestCaseData("line1\r\nline2\n", new string[] { "line1", "\r\n", "line2", "\n" })
    ];

    [TestCaseSource(nameof(_testCases))]
    public void TestFileContentParser(string content, ICollection<string> expectedTokens)
    {
        FileContentParser parser = new(content);
        List<string> tokensParsed = [];
        string token;
        while ((token = parser.Parse()) != null)
        {
            tokensParsed.Add(token);
        }
        Assert.That(tokensParsed, Is.EqualTo(expectedTokens));
    }
}
