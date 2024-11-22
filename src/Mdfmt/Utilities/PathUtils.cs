using System.Collections.Generic;
using System.IO;

namespace Mdfmt.Utilities;

public static class PathUtils
{
    /// <summary>
    /// Make a path that is relative to another.
    /// </summary>
    /// <param name="relativeTo">
    ///   The Path option passed to Mdfmt program.  This is either the path of a directory to 
    ///   process, or it is the path of a specific Markdown file to process.
    /// </param>
    /// <param name="filePath">
    ///   File path that needs to be made relative.
    /// </param>
    /// <returns>
    ///   Relative version of filePath passed in.
    /// </returns>
    public static string MakeRelative(string relativeTo, string filePath)
    {
        string filename = Path.GetFileName(filePath);
        string temporaryPath = Path.GetRelativePath(relativeTo, filePath).Replace("\\", "/");
        temporaryPath = temporaryPath == "." ? filename : temporaryPath;
        string relativePath = "./" + temporaryPath;
        return relativePath;
    }

    /// <summary>
    /// Converts the rpath to a canonical path, based on the cpath argument for context.
    /// </summary>
    /// <param name="cpath">Canonical relative file path, or cpath</param>
    /// <param name="rpath">
    /// Relative path, rooted at cpath's containing directory, to another file in scope of the mdfmt run
    /// </param>
    /// <returns>
    /// rpath expressed as a canonical path, or null if it is not possible to do this because it
    /// "busts out" of the directory where we are working.
    /// </returns>
    //NOTE: This method is well tested and it works great.  It becomes important if you are trying to work
    // with relative paths between files, and each file is making relative paths from its own point of
    // view.  Then this method helps you create a canonical path, based on the Path option passed to
    // mdfmt.   See README.md file for concepts and test cases, if we ever want to
    // pic this up again.  -SF 11/5/2024.
    public static string Canonicalize(string cpath, string rpath)
    {
        // Step 1: Get the directory of cpath as the base for relative path resolution
        string baseDirectory = Path.GetDirectoryName(cpath);

        // Step 2: Combine the base directory with rpath
        string combinedPath = Path.Combine(baseDirectory, rpath);

        // Step 3: Normalize the combined path to get the canonical path
        string canonicalPath = Path.GetFullPath(combinedPath);

        // Step 4: Get the full path of the root directory to check boundaries
        string rootPath = Path.GetFullPath("./");

        // Step 5: Check if the canonical path is within the root directory
        if (!canonicalPath.StartsWith(rootPath))
            return null;

        // Step 6: Trim the root path prefix to format it as a canonical path
        canonicalPath = "./" + canonicalPath[rootPath.Length..].TrimStart(Path.DirectorySeparatorChar);

        // Step 7: Replace backslashes with forward slashes
        canonicalPath = canonicalPath.Replace('\\', '/');

        return canonicalPath;
    }

    /// <summary>
    /// <para>
    /// Returns all the <em>left paths</em> of the path passed in, in descending length order.  A
    /// <em>left path</em> is a path that is rooted on the left but may have 0 or more path
    /// segments chopped off on the right.
    /// </para>
    /// <para>
    /// <b>Example:</b> <c>path == "./foo/bar/baz.md"</c>
    /// <list type="number">
    /// <item><c>./foo/bar/baz.md</c></item>
    /// <item><c>./foo/bar</c></item>
    /// <item><c>./foo</c></item>
    /// <item><c>.</c></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="path">A path</param>
    /// <returns>IEnumerable for getting all of the left paths of the path.</returns>
    public static IEnumerable<string> LeftPaths(string path)
    {
        string currentPath = path;
        while (!string.IsNullOrEmpty(currentPath))
        {
            yield return currentPath.Replace('\\', '/');
            currentPath = Path.GetDirectoryName(currentPath) ?? ".";
        }
    }

}
