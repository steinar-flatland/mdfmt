using System.IO;

namespace Mdfmt.Utilities;

internal class PathUtils(string path)
{
    /// <summary>
    /// The Path option passed to the program.  This is either the path of a directory to process,
    /// or it is the path of of a specific Markdown file to process.
    /// </summary>
    private readonly string _path = path;

    /// <summary>
    /// Make a relative path based on the filePath passed in.  The resulting relative path starts
    /// with "./" and contains all forward slashes, where '.' refers to either the directory that
    /// was passed to the program's Path option, or it refers to the directory that contains the
    /// specific file that was passed to the program's Path option.
    /// </summary>
    /// <param name="filePath">File path that needs to be made relative</param>
    /// <returns>relative version of filePath passed in</returns>
    public string MakeRelative(string filePath)
    {
        string filename = Path.GetFileName(filePath);
        string temporaryPath = Path.GetRelativePath(_path, filePath).Replace("\\", "/");
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
        canonicalPath = "./" + canonicalPath.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar);

        // Step 7: Replace backslashes with forward slashes
        canonicalPath = canonicalPath.Replace('\\', '/');

        return canonicalPath;
    }
}
