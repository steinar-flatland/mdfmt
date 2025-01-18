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
    public const string Version = "1.1.0";

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
            Parser parser = new(with =>
            {
                with.CaseInsensitiveEnumValues = true;
            });
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
        parsedResult.WithParsed(options =>
        {
            ValidateOptions(options, out string processingRoot, out string mdfmtFilePath);
            MdfmtProfile mdfmtProfile = (mdfmtFilePath == null) ? null : MdfmtProfileLoader.Load(mdfmtFilePath);
            MdfmtOptions mdfmtOptions = new(args, options, processingRoot, mdfmtProfile);
            Processor processor = new(mdfmtOptions);
            processor.Run();
            throw new ExitException(ExitCodes.Success);
        });
    }

    /// <summary>
    /// Validate that the command line options passed to the program are valid.
    /// Throws an <see cref="ExitException"/> if validation fails.
    /// </summary>
    /// <param name="commandLineOptions">
    /// The <see cref="CommandLineOptions"/> to validate.
    /// </param>
    /// <param name="processingRoot">
    /// Output parameter that is set to a full path defining the root of the files the Mdfmt can see
    /// and process.  Always returns a full path, never null.
    /// </param>
    /// <param name="mdfmtFilePath">
    /// Output parameter that is set to the full path of .mdfmt file, or null if there is none.
    /// When non-null, the path indicates a file that is an immediate descendant of the <c>processingRoot</c>.
    /// </param>
    /// <exception cref="ExitException"/>
    private static void ValidateOptions(CommandLineOptions commandLineOptions, out string processingRoot, out string mdfmtFilePath)
    {
        // Make sure the path is either a directory, or a file whose name ends in .md
        ValidateTargetPath(commandLineOptions.TargetPath, out bool targetPathIsDirectory);

        // Validate the rest of the options.
        CommandLineOptionsValidator validator = new();
        try
        {
            validator.ValidateAndThrow(commandLineOptions);
        }
        catch (ValidationException ex)
        {
            Output.Error(ex.Message);
            throw new ExitException(ExitCodes.MisuseOfCommand);
        }

        // The goal is to set these output parameters.
        // processingRoot always gets a full path as a value.
        // mdfmtFilePath will get a full path or may remain null if no .mdfmt file is available.
        processingRoot = null;
        mdfmtFilePath = null;

        string targetDirectory = targetPathIsDirectory ? commandLineOptions.TargetPath : Path.GetDirectoryName(commandLineOptions.TargetPath);
        DirectoryInfo candidateProcessingRootDirectoryInfo = new(targetDirectory);
        do
        {
            string candidateProcessingRoot = candidateProcessingRootDirectoryInfo.FullName;
            string candidateMdfmtFilePath = Path.Combine(candidateProcessingRoot, ".mdfmt");
            if (File.Exists(candidateMdfmtFilePath))
            {
                processingRoot = candidateProcessingRoot;
                mdfmtFilePath = candidateMdfmtFilePath;
                break;
            }
        } while ((candidateProcessingRootDirectoryInfo = candidateProcessingRootDirectoryInfo.Parent) != null);
        processingRoot ??= new DirectoryInfo(targetDirectory).FullName;
    }

    /// <summary>
    /// Validate that a string, <c>targetPath</c>, is either the path of a directory or of a .md file.
    /// Throws an <see cref="ExitException"/> if validation fails.
    /// </summary>
    /// <param name="targetPath">
    /// The string to validate.  This path can be either relative to the current working directory
    /// or absolute.
    /// </param>
    /// <param name="targetPathIsDirectory">
    /// Output parameter that is set to <c>true</c> if <c>targetPath</c> is a directory or else to
    /// <c>false</c> if it is a file.</param>
    /// <exception cref="ExitException"/>
    private static void ValidateTargetPath(string targetPath, out bool targetPathIsDirectory)
    {
        if (Directory.Exists(targetPath))
        {
            targetPathIsDirectory = true;
            return;
        }
        else if (File.Exists(targetPath))
        {
            if (targetPath.EndsWith(".md"))
            {
                targetPathIsDirectory = false;
                return;
            }
            else
            {
                Output.Error("File cannot be processed because it is not a .md file");
                throw new ExitException(ExitCodes.MisuseOfCommand);
            }
        }
        else
        {
            Output.Error($"{targetPath} does not exist");
            throw new ExitException(ExitCodes.MisuseOfCommand);
        }
    }

    private static void HandleNotParsed(ParserResult<CommandLineOptions> parsedResult)
    {
        parsedResult.WithNotParsed(errors =>
        {
            if (errors.IsHelp())
            {
                HelpText helpText = HelpText.AutoBuild(parsedResult, h => {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = $"Mdfmt version {Version}";
                    h.Copyright = "Copyright (c) 2024-2025 Steinar Flatland.\nThis software is licensed under the Apache License 2.0.\nSee https://github.com/steinar-flatland/mdfmt/blob/main/LICENSE for details.";
                    return h;
                });
                Output.Info(helpText);
                throw new ExitException(ExitCodes.Success);
            }
            else if (errors.IsVersion())
            {
                Output.Info(Version);
                throw new ExitException(ExitCodes.Success);
            }
            else
            {
                Output.Error("Error parsing arguments:");
                foreach (Error error in errors)
                {
                    Output.Error(error);
                }
                throw new ExitException(ExitCodes.MisuseOfCommand);
            }
        });
    }

    private static void LogException(Exception ex)
    {
        Output.Error($"Error: {ex.GetType().Name}. {ex.Message}");
        if (ex is not JsonException)
        {
            Output.Error($"Stack Trace:{Environment.NewLine}{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Output.Error("Inner Exception:");
                Output.Error($"Message: {ex.InnerException.Message}");
                Output.Error($"Stack Trace:{Environment.NewLine}{ex.InnerException.StackTrace}");
            }
        }
    }
}
