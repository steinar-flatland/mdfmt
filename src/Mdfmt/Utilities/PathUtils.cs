using System;
using System.Collections.Generic;
using System.IO;

namespace Mdfmt.Utilities;

internal static class PathUtils
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
    /// <para>
    /// Given two relative paths to files that are in scope of the Mdfmt run where: (1) <c>cpath</c>
    /// is relative to the processing root directory, and (2) <c>rpath</c> is relative to the
    /// directory that contains the file represented by <c>cpath</c>, return a new path to the 
    /// file of <c>rpath</c> expressed relative to the processing root.  (Note that the processing
    /// root is the directory that defines the root of the files that are in scope for the Mdfmt run.)
    /// This method is called <c>Canonicalize</c> because it converts the <c>rpath</c> argument such
    /// that it is expressed as being relative to the processing root.  Mdfmt considers this to be
    /// the standard, or canonical, way of expressing a relative path.
    /// </para>
    /// <para>
    /// Throws an <see cref="ArgumentException"/> if <c>cpath</c> does not start with <c>"./"</c>.
    /// </para>
    /// </summary>
    /// <param name="cpath">
    /// The relative path of a file that is in scope of the Mdfmt run, expressed as a path relative
    /// to the processing root directory.  (The processing root is the root directory that defines
    /// the scope of the files Mdfmt can see.)  The <c>cpath</c> MUST start with <c>"./"</c>, where
    /// the dot represents the processing root directory, and if this is not the case, an
    /// <see cref="ArgumentException"/> results.
    /// </param>
    /// <param name="rpath">
    /// The relative path of a file that is in scope of the Mdfmt run, expressed as a path relative
    /// to the directory that contains the file represented by <c>cpath</c>.  <c>rpath</c> should
    /// start with either <c>"./"</c> or <c>"../"</c>, and if it does not then it is adjusted by
    /// prepending <c>"./"</c>.
    /// </param>
    /// <returns>
    /// <c>rpath</c> expressed as a canonical path, or null if it is not possible to do this because
    /// <c>rpath</c> has too many <c>../</c> segments and "busts out" of the processing root, or
    /// otherwise the <c>rpath</c> passed in is invalid.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    public static string Canonicalize(string cpath, string rpath)
    {
        // A canonical relative path MUST alwasy start "./". 
        // . represents the processing root directory.
        if (!cpath.StartsWith("./"))
        {
            throw new ArgumentException("Value must start with \"./\".", nameof(cpath));
        }

        // Rpaths come in "from the wild".  We'll forgive if they don't start with a dot and
        // make an adjustment.
        if (!rpath.StartsWith('.'))
        {
            rpath = $"./{rpath}";
        }

        if (rpath.StartsWith("./"))
        {
            return $"{cpath[..(cpath.LastIndexOf('/') + 1)]}{rpath[2..]}";
        }
        else if (rpath.StartsWith("../"))
        {
            int parentCount = ParentCount(rpath);
            for (int i = cpath.Length - 1, n = parentCount + 1; i >= 0; i--)
            {
                char c = cpath[i];
                if (c == '/')
                {
                    n--;
                    if (n == 0)
                    {
                        return $"{cpath[..i]}/{rpath[(3*parentCount)..]}";
                    }
                }
            }
            // rpath goes up too many directories and "busts out" of the processing root.
            return null;
        }
        else
        {
            // Signal that Canonicalization was unsuccessful.
            return null;
        }
    }

    /// <summary>
    /// Given a path string, count the number of "../" at the beginning of the string.
    /// </summary>
    /// <param name="path">Path string</param>
    /// <returns>Count of number of "../" at beginning of path</returns>
    private static int ParentCount(string path)
    {
        int count = 0;
        int pos = 0;
        while (pos + 3 <= path.Length &&
               path[pos] == '.' &&
               path[pos + 1] == '.' &&
               path[pos + 2] == '/')
        {
            count++;
            pos += 3;
        }
        return count;
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
