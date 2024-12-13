using CommandLine;
using CommandLine.Text;
using FluentValidation;
using Mdfmt.Options;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("Unit.Mdfmt")]
[assembly: InternalsVisibleTo("Integration.Mdfmt")]

namespace Mdfmt;

internal class Program
{
    private const string Version = "0.3.1";

    public static void Main(string[] args)
    {
        try
        {
            RunProgram(args);
        }
        catch (Exception ex)
        {
            LogException(ex);
            Environment.Exit(ExitCodes.GeneralError);
        }
    }

    /// <summary>
    /// <para>
    /// Run the program with the specified command line arguments and exit behavior.  This method
    /// is designed for testability.
    /// </para>
    /// <para>
    /// In a production run, the default value of <c>doExit = true</c> is used.  Then, any
    /// <c>ExitException</c> that is thrown causes a call to <c>Environment.Exit()</c>,
    /// terminating the CLI, passing the exit code to the operating system.
    /// </para>
    /// <para>
    /// In a test run, use <c>doExit = false</c>, which causes the exit code to be returned to the
    /// calling test rather than terminating the current process, which would be disruptive to
    /// testing.
    /// </para>
    /// </summary>
    /// <param name="args">The command line arguments to the program.</param>
    /// <param name="doExit">Whether to call <c>Environment.Exit()</c> if an <c>ExitException</c>
    /// is trapped.  Defaults to <c>true</c>.</param>
    /// <returns>Exit code when <c>doExit == false</c>.</returns>
    public static int RunProgram(string[] args, bool doExit = true)
    {
        try
        {
            Parser parser = new(with => with.CaseInsensitiveEnumValues = true);
            ParserResult<CommandLineOptions> parsedResult = parser.ParseArguments<CommandLineOptions>(args);
            HandleParsed(args, parsedResult);
            HandleNotParsed(parsedResult);
        }
        catch (ExitException ex)
        {
            if (doExit)
                Environment.Exit(ex.ExitCode);
            return ex.ExitCode;
        }
        if (doExit)
            Environment.Exit(ExitCodes.Success);
        return ExitCodes.Success;
    }

    private static void HandleParsed(string[] args, ParserResult<CommandLineOptions> parsedResult)
    {
        MdfmtProfile mdfmtProfile = null;

        parsedResult.WithParsed(options =>
        {
            if (Directory.Exists(options.Path))
            {
                string mdfmtFileName = Path.Combine(options.Path, ".mdfmt");
                if (File.Exists(mdfmtFileName))
                {
                    mdfmtProfile = MdfmtProfileLoader.Load(mdfmtFileName);
                }
            }
            else
            {
                if (!File.Exists(options.Path))
                {
                    Console.WriteLine($"{options.Path} does not exist.");
                    throw new ExitException(ExitCodes.GeneralError);
                }
                else
                {
                    if (!options.Path.EndsWith(".md"))
                    {
                        Console.WriteLine("File cannot be processed because it is not a .md file");
                        throw new ExitException(ExitCodes.GeneralError);
                    }
                }
            }

            // Ensure options are valid.
            CommandLineOptionsValidator validator = new();
            try
            {
                validator.ValidateAndThrow(options);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine(ex.Message);
                throw new ExitException(ExitCodes.MisuseOfCommand);
            }

            MdfmtOptions mdfmtOptions = new(args, options, mdfmtProfile);

            Processor processor = new(mdfmtOptions);
            processor.Run();
            throw new ExitException(ExitCodes.Success);
        });
    }

    private static void HandleNotParsed(ParserResult<CommandLineOptions> parsedResult)
    {
        parsedResult.WithNotParsed(errors =>
        {
            if (errors.IsHelp())
            {
                Console.WriteLine("Displaying help:");
                HelpText helpText = HelpText.AutoBuild(parsedResult);
                Console.WriteLine(helpText);
                throw new ExitException(ExitCodes.Success);
            }
            else if (errors.IsVersion())
            {
                Console.WriteLine(Version);
                throw new ExitException(ExitCodes.Success);
            }
            else
            {
                Console.WriteLine("Error parsing arguments.");
                foreach (Error error in errors)
                {
                    Console.WriteLine(error);
                }
                throw new ExitException(ExitCodes.MisuseOfCommand);
            }
        });
    }

    private static void LogException(Exception ex)
    {
        Console.WriteLine($"Error: {ex.GetType().Name}. {ex.Message}");
        if (ex is not JsonException)
        {
            Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception:");
                Console.WriteLine($"Message: {ex.InnerException.Message}");
                Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.InnerException.StackTrace}");
            }
        }
    }
}
