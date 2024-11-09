using Mdfmt.Options;
using System;

namespace Mdfmt.Loaders;

internal static class Newline
{
    public const string Unix = "\n";
    public const char UnixChar = '\n';
    public const string Windows = "\r\n";

    /// <summary>
    /// Determine the kind of newline characters to use, considering both the --newline-strategy
    /// command line option and the kinds of newlines present in the file being parsed.
    /// </summary>
    /// <param name="newlineStrategy">Command line option selected by user</param>
    /// <param name="fileContent">File content being processed</param>
    /// <param name="rewriteRequired">Output parameter for whether the file needs to be written to enforce the user's preference</param>
    /// <returns>A string that is either <c>"\n"</c> or <c>"\r\n"</c>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string NewlineSequence(NewlineStrategy newlineStrategy, string fileContent, out bool rewriteRequired)
    {
        int unixNewlineCount = UnixNewlineCount(fileContent);
        int windowsNewlineCount = WindowsNewlineCount(fileContent);

        switch (newlineStrategy)
        {
            case NewlineStrategy.Unix:
                rewriteRequired = windowsNewlineCount != 0;
                return Unix;
            case NewlineStrategy.Windows:
                rewriteRequired = unixNewlineCount != 0;
                return Windows;
            case NewlineStrategy.PreferUnix:
                if (windowsNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Unix;
                }
                else if (unixNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Windows;
                }
                else
                {
                    rewriteRequired = true;
                    return Unix;
                }
            case NewlineStrategy.PreferWindows:
                if (unixNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Windows;
                }
                else if (windowsNewlineCount == 0)
                {
                    rewriteRequired = false;
                    return Unix;
                }
                else
                {
                    rewriteRequired = true;
                    return Windows;
                }
            default:
                throw new InvalidOperationException($"Unhandled {nameof(NewlineStrategy)}: {newlineStrategy}");
        }
    }

    private static int UnixNewlineCount(string fileContent)
    {
        int count = 0;
        for (int i = 0; i < fileContent.Length; i++)
            if (fileContent[i] == '\n' && (i == 0 || fileContent[i - 1] != '\r'))
                count++;
        return count;
    }

    private static int WindowsNewlineCount(string fileContent)
    {
        int count = 0;
        int index = 0;
        while ((index = fileContent.IndexOf(Windows, index)) != -1)
        {
            count++;
            index += Windows.Length;
        }
        return count;
    }
}
