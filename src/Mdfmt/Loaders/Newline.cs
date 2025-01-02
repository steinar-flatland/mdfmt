using Mdfmt.Options;
using System;

namespace Mdfmt.Loaders;

internal static class Newline
{
    /// <summary>
    /// <para>
    /// Determine the kind of newline characters to use, considering both the --newline-strategy
    /// command line option (or null if it was not provided) and the kinds of newlines present in
    /// the file being parsed.
    /// </para>
    /// <para>
    /// Throws a <c>NotImplementedException</c> if the <c>newlineStrategy</c> passed in is
    /// unhandled.  This really should not happen and would indicate a code maintenance error.
    /// </para>
    /// </summary>
    /// <param name="newlineStrategy">
    /// Enum value communicating user's preferred strategy for managing newlines, or null if no 
    /// preference.
    /// </param>
    /// <param name="fileContent">
    /// File content being processed.
    /// </param>
    /// <param name="rewriteRequired">
    /// Output parameter for whether the file needs to be written to enforce the user's preference.
    /// </param>
    /// <returns>
    /// A string that is either <c>"\n"</c> or <c>"\r\n"</c>.
    /// </returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string DetermineNewline(NewlineStrategy? newlineStrategy, string fileContent, out bool rewriteRequired)
    {
        int unixNewlineCount = UnixNewlineCount(fileContent);
        int windowsNewlineCount = WindowsNewlineCount(fileContent);

        switch (newlineStrategy)
        {
            case NewlineStrategy.Unix:
                rewriteRequired = windowsNewlineCount != 0;
                return Constants.UnixNewline;
            case NewlineStrategy.Windows:
                rewriteRequired = unixNewlineCount != 0;
                return Constants.WindowsNewline;
            case NewlineStrategy.PreferUnix:
                if (windowsNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Constants.UnixNewline;
                }
                else if (unixNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Constants.WindowsNewline;
                }
                else
                {
                    rewriteRequired = true;
                    return Constants.UnixNewline;
                }
            case NewlineStrategy.PreferWindows:
                if (unixNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Constants.WindowsNewline;
                }
                else if (windowsNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Constants.UnixNewline;
                }
                else
                {
                    rewriteRequired = true;
                    return Constants.WindowsNewline;
                }
            case null:
                rewriteRequired = false;
                return unixNewlineCount >= windowsNewlineCount ? Constants.UnixNewline : Constants.WindowsNewline;
            default:
                throw new NotImplementedException($"Unhandled {nameof(NewlineStrategy)}: {newlineStrategy}");
        }
    }

    /// <summary>
    /// Determine the number of Unix newlines in a string of file content.
    /// (Only count a newline character if it is the first character in the file
    /// or it is not preceded by a carriage return.)
    /// </summary>
    /// <param name="fileContent">The contents of a file.</param>
    /// <returns>The Unix newline count.</returns>
    private static int UnixNewlineCount(string fileContent)
    {
        int count = 0;
        for (int i = 0; i < fileContent.Length; i++)
            if ((fileContent[i] == '\n') && (i == 0 || fileContent[i - 1] != '\r'))
                count++;
        return count;
    }

    /// <summary>
    /// Determine the number of Windows newlines in a string of file content.
    /// </summary>
    /// <param name="fileContent">The contents of a file.</param>
    /// <returns>The Windows newline count.</returns>
    private static int WindowsNewlineCount(string fileContent)
    {
        int count = 0;
        int index = 0;
        while ((index = fileContent.IndexOf(Constants.WindowsNewline, index)) != -1)
        {
            count++;
            index += Constants.WindowsNewline.Length;
        }
        return count;
    }
}
