using System;
using System.Collections.Generic;

namespace Mdfmt.Options;

/// <summary>
/// This class combines various sources of knowledge about configuration, to produce the options
/// that Mdfmt relies on to know what to do.
/// </summary>
/// <param name="args">
/// The raw command line arguments passed to the program.
/// </param>
/// <param name="commandLineOptions">
/// <c>CommandLineOptions</c> instance that was parsed from the raw command line arguments by the 
/// <c>CommandLineParser</c> NuGet package.
/// </param>
/// <param name="mdfmtProfile">
/// Information from a <c>.mdfmt</c> JSON file that was loaded from the root of the Mdfmt context.
/// This is only provided if the positional argument to the <c>CommandLineParser</c> is a
/// directory, and that directory immediately contains (i.e., not through a subdirectory)
/// a file named <c>.mdfmt</c>.  If no <c>.mdfmt</c> file is available, this is <c>null</c>.
/// </param>
public class MdfmtOptions
    (
        string[] args,
        CommandLineOptions commandLineOptions,
        MdfmtProfile mdfmtProfile
    )
{
    private readonly HashSet<string> _argNames = GetNamesOf(args);
    private readonly CommandLineOptions _commandLineOptions = commandLineOptions;
    private readonly MdfmtProfile _mdfmtProfile = mdfmtProfile;
    private readonly FileProcessingOptions _commandLineFileProcessingOptions = FileProcessingOptionsOf(commandLineOptions);

    private static HashSet<string> GetNamesOf(string[] args)
    {
        HashSet<string> argNames = [];
        foreach (string arg in args)
        {
            if (arg.StartsWith('-'))
            {
                argNames.Add(arg);
            }
        }
        return argNames;
    }

    private static FileProcessingOptions FileProcessingOptionsOf(CommandLineOptions commandLineOptions)
    {
        FileProcessingOptions fileProcessingOptions = new()
        {
            Platform = commandLineOptions.Platform,
            HeadingNumbering = commandLineOptions.HeadingNumbering,
            MinimumEntryCount = commandLineOptions.MinimumEntryCount,
            NewlineStrategy = commandLineOptions.NewlineStrategy
        };
        return fileProcessingOptions;
    }

    public string Path => _commandLineOptions.Path;

    public bool Verbose => _commandLineOptions.Verbose;

    public bool Recursive => _commandLineOptions.Recursive;

    /// <summary>
    /// Get the options that determine how a specific Markdown file is processed.
    /// </summary>
    /// <param name="cpath">
    /// The canonical relative path of a Markdown file, from the root of the Mdfmt context.
    /// </param>
    /// <returns>
    /// The file procssing options to use for the specified file.
    /// </returns>
    public FileProcessingOptions GetFileProcessingOptions(string cpath)
    {
        // If a .mdfmt file was loaded into _mdfmtProfile, and it maps the cpath to an instance of
        // FileProcessingOptions, then return based on that.
        if (_mdfmtProfile != null && _mdfmtProfile.TryGetFileProcessingOptions(cpath, out var fileProcessingOptions))
        {
            // If there are file processing options that were explicitly set on the command line,
            // then those override the options from the _mdfmtProfile.
            OverwriteExplicitlySetCommandLineOptionsOnto(fileProcessingOptions);

            // If the file processing options are incomplete, backfill from the command line.
            // This guarantees that the returned FileProcessingOptions are complete (i.e., no null options)
            if (!fileProcessingOptions.IsComplete())
            {
                Console.WriteLine("Warning: Backfilling mdfmt options from command line");
                fileProcessingOptions.FillGapsFrom(_commandLineFileProcessingOptions);
            }

            return fileProcessingOptions;
        }
        // In all other cases, simply return FileProcessingOptions based on the values passed to the
        // command line.
        else
        {
            return _commandLineFileProcessingOptions;
        }
    }

    private void OverwriteExplicitlySetCommandLineOptionsOnto(FileProcessingOptions fileProcessingOptions)
    {
        if (_argNames.Contains("-p") || _argNames.Contains("--platform"))
        {
            fileProcessingOptions.Platform = _commandLineFileProcessingOptions.Platform;
        }
        if (_argNames.Contains("-n") || _argNames.Contains("--heading-numbers"))
        {
            fileProcessingOptions.HeadingNumbering = _commandLineFileProcessingOptions.HeadingNumbering;
        }
        if (_argNames.Contains("--minimum-entry-count"))
        {
            fileProcessingOptions.MinimumEntryCount = _commandLineFileProcessingOptions.MinimumEntryCount;
        }
        if (_argNames.Contains("--newline-strategy"))
        {
            fileProcessingOptions.NewlineStrategy = _commandLineFileProcessingOptions.NewlineStrategy;
        }
    }
}
