﻿using System.Collections.Generic;
using Mdfmt.Utilities;

namespace Unit.Mdfmt.Utilities;

[TestFixture]
public class PathUtilsTests
{
    private readonly static object[] _makeRelativeTestCases =
    [
        new object[] { "./file.md", "./file.md", "./file.md" },
        new object[] { "C:\\My\\docs\\a\\b\\file.md", "C:\\My\\docs\\a\\b\\file.md", "./file.md" },
        new object[] { "C:\\My\\docs", "C:\\My\\docs\\a\\b\\file.md", "./a/b/file.md" },
        new object[] { "C:\\My\\docs", "C:\\My\\docs\\file.md", "./file.md" },
        new object[] { ".", ".\\file.md", "./file.md" },
        new object[] { "./a/b/c", "./a/b/c/d/file.md", "./d/file.md" },
        new object[] { ".\\a\\b\\c", "./a/b/c/d/file.md", "./d/file.md" },
    ];

    [TestCaseSource(nameof(_makeRelativeTestCases))]
    public void MakeRelativeTest(string relativeTo, string filePath, string expectedResult)
    {
        string result = PathUtils.MakeRelative(relativeTo, filePath);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private readonly static object[] _leftPathsTestCases =
    [
        new object[] { "./foo/bar/baz.md", new string[] {"./foo/bar/baz.md", "./foo/bar", "./foo", "."}  },
        new object[] { "./file.md", new string[] { "./file.md", "." } },
        new object[] { "/a/b/c/d", new string[] { "/a/b/c/d", "/a/b/c", "/a/b", "/a", "/", "." } },
        new object[] { "hello", new string[] { "hello" } },
        new object[] { ".", new string[] { "." } },
        new object[] { string.Empty, System.Array.Empty<string>() },
    ];

    [TestCaseSource(nameof(_leftPathsTestCases))]
    public void LeftPathsTest(string path, IEnumerable<string> expectedLeftPaths)
    {
        IEnumerable<string> leftPaths = PathUtils.LeftPaths(path);
        Assert.That(leftPaths, Is.EquivalentTo(expectedLeftPaths));
    }

    private readonly static object[] _canonicalizeTestCases =
    [
        new object[] { "./B/D/d", "../E/H/h", "./B/E/H/h" },
        new object[] { "./a", "./C/c", "./C/c" },
        new object[] { "./a", "./C/G/g", "./C/G/g" },
        new object[] { "./B/E/H/h", "../../../C/F/I/J/j", "./C/F/I/J/j" },
        new object[] { "./C/F/f", "./I/J/j", "./C/F/I/J/j" },
        new object[] { "./C/F/I/i", "../../G/g", "./C/G/g" },
        new object[] { "./A/a", "b", "./A/b" },
        new object[] { "./a", "../b", null },
        new object[] { "./docs/user/README.md", "../../../../mdfmt/docs/developer/Windows-Laptop-Setup-Instructions.md", null },
        new object[] { "./file1.md", "file2.md", "./file2.md"}
    ];

    [TestCaseSource(nameof(_canonicalizeTestCases))]
    public void CanonicalizeTest(string cpath, string rpath, string expectedResult)
    {
        string result = PathUtils.Canonicalize(cpath, rpath);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

}
