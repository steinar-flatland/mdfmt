using System;

namespace Mdfmt;

/// <summary>
/// When you want to exit the program, throw this exception.  There is a top-level handler in
/// <c>Program.cs</c> that does the right thing with this Exception.  It is important to throw
/// this exception rather than calling <c>Environment.Exit()</c> directly, to maintain testability.
/// </summary>
internal class ExitException : Exception
{
    public int ExitCode { get; }

    public ExitException(int exitCode, string message)
        : base(message)
    {
        ExitCode = exitCode;
    }

    public ExitException(int exitCode)
        : base($"{nameof(ExitException)} with exit code {exitCode}.")
    {
        ExitCode = exitCode;
    }
}
