namespace Mdfmt;

/// <summary>
/// Exit codes returned from the program to the shell.
/// </summary>
internal static class ExitCodes
{
    /// <summary>
    /// Program exited successfully.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Program exited with error.  This covers most errors except for the more specific class of
    /// error represented by <c>MisuseOfCommand</c>.
    /// </summary>
    public const int GeneralError = 1;

    /// <summary>
    /// Incorrect command line arguments when invoking the program.
    /// </summary>
    public const int MisuseOfCommand = 2;
}
