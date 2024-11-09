# Possible Tests for NormalLineParser

`Mdfmt.Loaders.NormalLineParser` is a sub-parser for a "normal" line of Markdown, not something more special like table of contents, fenced code block, or line in an indented code block.  

This parser is responsible for splitting the line into different regions.  Right now this is necessitated only by the desire to respect HTML comments that could be embedded.

It was a bit tricky to get this parser right.  Here are some test cases that helped.  These need to be turned into actual unit tests.

```C#
    private static void TestParser(bool inHtmlComment, string line)
    {
        NormalLineParseResult result = NormalLineParser.Parse(inHtmlComment, line);
        Console.WriteLine();
        Console.WriteLine($"Parse(inHtmlComment:{inHtmlComment}, line:\"{line}\")");
        Console.WriteLine($"  {result}");
        Console.WriteLine($"  result.AtomicRegions.Count: {result.AtomicRegions.Count}");
        Console.WriteLine($"  result.MutableContent: \"{result.MutableContent}\"");
        Console.WriteLine($"  result.IsHeading: {result.IsHeading}");
    }

    public static void Main(string[] args)
    {
        TestParser(false, "");
        TestParser(true, "");
        TestParser(false, "1.0 Introduction");
        TestParser(false, "## 1.0 Introduction");
        TestParser(false, "## 1.0 Introduction <!--of My Document");
        TestParser(false, "## 1.0 Introduction of <!--My -->Document");
        TestParser(false, "## 1.0 Introduction of <!--My -->Document<!---->");
        TestParser(true, "-->## Eeked out a heading");
        TestParser(false, "-->## Not a heading (subtle)");
        TestParser(false, "<!--go into a comment right away!");
        TestParser(false, "<!--# This heading has been commented out-->");
    }
```
