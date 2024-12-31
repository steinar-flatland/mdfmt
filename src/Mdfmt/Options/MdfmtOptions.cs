using System;
using System.Collections.Generic;
using System.Text;

namespace Mdfmt.Options;

//NOTE: This class must be in sync with class CommandLineOptions.  This class builds on CommandLineOptions.

/// <summary>
/// This class combines various sources of knowledge about configuration, to produce the options
/// that Mdfmt relies on to know what to do.
/// </summary>
/// <param name="args">
/// The raw command line arguments passed to the program.
/// </param>
/// <param name="commandLineOptions">
/// <see cref="CommandLineOptions"/> instance that was parsed from the raw command line arguments by the 
/// <c>CommandLineParser</c> NuGet package.
/// </param>
/// <param name="mdfmtProfile">
/// Information from a <c>.mdfmt</c> JSON file that was loaded from the directory being processed by Mdfmt.
/// This is only provided if the positional argument to the command line parser is a
/// directory, and that directory immediately contains (i.e., not through a subdirectory)
/// a file named <c>.mdfmt</c>.  If no <c>.mdfmt</c> file is available, this is <c>null</c>.
/// </param>
internal class MdfmtOptions
    (
        string[] args,
        CommandLineOptions commandLineOptions,
        MdfmtProfile mdfmtProfile
    )
{
    /// <summary>
    /// Names of arguments that were explicitly set on the command line.  This knowledge is used to
    /// give precedence to explicitly set command line arguments, overriding values from .mdfmt file.
    /// </summary>
    private readonly HashSet<string> _argNames = GetNamesOf(args);

    /// <summary>
    /// <see cref="CommandLineOptions"/> instance that was parsed from the raw command line arguments by the 
    /// <c>CommandLineParser</c> NuGet package.
    /// </summary>
    private readonly CommandLineOptions _commandLineOptions = commandLineOptions;

    /// <summary>
    /// Data structure with information loaded from .mdfmt file, or null if there is no such profile.
    /// </summary>
    private readonly MdfmtProfile _mdfmtProfile = mdfmtProfile;

    /// <summary>
    /// Instance of <see cref="FileProcessingOptions"/> instantiated based on command line options.
    /// This knowledge is used as a fallback mechanism:  Any file processing options not 
    /// specified through the .mdfmt file are taken from here as a fallback.
    /// </summary>
    private readonly FileProcessingOptions _commandLineFileProcessingOptions = FileProcessingOptionsOf(commandLineOptions);

    /// <summary>
    /// Build and return a set of argument names that were specified on the command line.
    /// </summary>
    /// <param name="args">Array of strings that are the command line arguments passed to the program</param>
    /// <returns>Set of argument names mentioned on the command line</returns>
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

    /// <summary>
    /// Create the fallback file-specific options, based on command line options.
    /// Use these options for processing a file, if more specific options are not
    /// determined through the .mdfmt file.
    /// </summary>
    /// <param name="commandLineOptions">options determined by parsing the program's command line</param>
    /// <returns><see cref="FileProcessingOptions"/></returns>
    private static FileProcessingOptions FileProcessingOptionsOf(CommandLineOptions commandLineOptions)
    {
        FileProcessingOptions fileProcessingOptions = new()
        {
            Flavor = commandLineOptions.Flavor,
            HeadingNumbering = commandLineOptions.HeadingNumbering,
            TocThreshold = commandLineOptions.TocThreshold,
            NewlineStrategy = commandLineOptions.NewlineStrategy
        };
        return fileProcessingOptions;
    }

    /// <summary>
    /// Directory in which to process .md files, or a specific .md file.
    /// </summary>
    public string Path => _commandLineOptions.Path;

    /// <summary>
    /// Whether to use verbose output.
    /// </summary>
    public bool Verbose => _commandLineOptions.Verbose;

    /// <summary>
    /// Whether to process .md files in all sub-folders.
    /// </summary>
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
            // If fileProcessingOptions has values that were overridden explicitly on the command line,
            // then write the values from the command into fileProcessingOptions.
            OverwriteExplicitlySetCommandLineOptionsOnto(fileProcessingOptions);

            // If the file processing options are incomplete, backfill from the command line,
            // to make the file processing options as complete as possible.
            if (!fileProcessingOptions.IsComplete())
            {
                fileProcessingOptions.PopulateFrom(_commandLineFileProcessingOptions);
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

    /// <summary>
    /// In the instance of <see cref="FileProcessingOptions"/> passed in, replace property values
    /// by explicit overrides from the command line.
    /// </summary>
    /// <param name="fileProcessingOptions">Instance of <see cref="FileProcessingOptions"/> to edit</param>
    private void OverwriteExplicitlySetCommandLineOptionsOnto(FileProcessingOptions fileProcessingOptions)
    {
        if (_argNames.Contains("-f") || _argNames.Contains("--flavor"))
        {
            fileProcessingOptions.Flavor = _commandLineFileProcessingOptions.Flavor;
        }
        if (_argNames.Contains("-h") || _argNames.Contains("--heading-numbers"))
        {
            fileProcessingOptions.HeadingNumbering = _commandLineFileProcessingOptions.HeadingNumbering;
        }
        if (_argNames.Contains("-t") || _argNames.Contains("--toc-threshold"))
        {
            fileProcessingOptions.TocThreshold = _commandLineFileProcessingOptions.TocThreshold;
        }
        if (_argNames.Contains("--newline-strategy"))
        {
            fileProcessingOptions.NewlineStrategy = _commandLineFileProcessingOptions.NewlineStrategy;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append($"{nameof(CommandLineOptions)}:{Environment.NewLine}");
        sb.Append(_commandLineOptions);
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);

        sb.Append($"Names of arguments explicitly passed to the command line:{Environment.NewLine}{(_argNames.Count == 0 ? "none" : string.Join(", ", _argNames))}");
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);

        sb.Append($"{nameof(MdfmtProfile)}:{Environment.NewLine}");
        sb.Append(_mdfmtProfile == null ? "none" : _mdfmtProfile);

        return sb.ToString();
    }
}
