﻿using System.Collections.Generic;

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
/// <param name="processingRoot">
/// Full path defining the root of files that Mdfmt can see and process.  If there is a
/// .mdfmt file, it is in this directory.
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
        string processingRoot,
        MdfmtProfile mdfmtProfile
    )
{
    /// <summary>
    /// Names of arguments that were explicitly set on the command line.  This knowledge is used to
    /// give precedence to explicitly set command line arguments, overriding values from .mdfmt file.
    /// </summary>
    public IReadOnlySet<string> ArgNames { get; } = GetNamesOf(args);

    /// <summary>
    /// <see cref="CommandLineOptions"/> instance that was parsed from the raw command line arguments by the 
    /// <c>CommandLineParser</c> NuGet package.
    /// </summary>
    public CommandLineOptions CommandLineOptions { get; } = commandLineOptions;

    /// <summary>
    /// Full path defining the root of files that Mdfmt can see and process.  If there is a
    /// .mdfmt file, it is in this directory.
    /// </summary>
    public string ProcessingRoot { get; } = processingRoot;

    /// <summary>
    /// Data structure with information loaded from .mdfmt file, or null if there is no such profile.
    /// </summary>
    public MdfmtProfile MdfmtProfile { get; } = mdfmtProfile;

    /// <summary>
    /// Instance of <see cref="FormattingOptions"/> instantiated based on command line options.
    /// This knowledge is used as a fallback mechanism:  Any formatting options not specified
    /// through the .mdfmt file are taken from here as a fallback.
    /// </summary>
    private readonly FormattingOptions _commandLineFormattingOptions = FormattingOptionsOf(commandLineOptions);

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
    /// Create fallback formatting options, based on command line options.
    /// Use these options for processing a file, if more specific options are not
    /// determined through the .mdfmt file.
    /// </summary>
    /// <param name="commandLineOptions">options determined by parsing the program's command line</param>
    /// <returns><see cref="FormattingOptions"/></returns>
    private static FormattingOptions FormattingOptionsOf(CommandLineOptions commandLineOptions)
    {
        FormattingOptions formattingOptions = new()
        {
            Flavor = commandLineOptions.Flavor,
            HeadingNumbering = commandLineOptions.HeadingNumbering,
            TocThreshold = commandLineOptions.TocThreshold,
            NewlineStrategy = commandLineOptions.NewlineStrategy
        };
        return formattingOptions;
    }

    /// <summary>
    /// Directory in which to process .md files, or a specific .md file.
    /// </summary>
    public string TargetPath => CommandLineOptions.TargetPath;

    /// <summary>
    /// Whether to use verbose output.
    /// </summary>
    public bool Verbose => CommandLineOptions.Verbose;

    /// <summary>
    /// Whether to process .md files in all sub-folders.
    /// </summary>
    public bool Recursive => CommandLineOptions.Recursive;

    /// <summary>
    /// Get the options that determine how a specific Markdown file should be formatted.
    /// </summary>
    /// <param name="cpath">
    /// The canonical relative path of a Markdown file, from the root of the Mdfmt context.
    /// </param>
    /// <returns>
    /// The formatting options to use for the specified file.
    /// </returns>
    public FormattingOptions GetFormattingOptions(string cpath)
    {
        // If a .mdfmt file was loaded into _mdfmtProfile, and it maps the cpath to an instance of
        // FormattingOptions, then return based on that.
        if (MdfmtProfile != null && MdfmtProfile.TryGetFormattingOptions(cpath, out var formattingOptions))
        {
            // If formattingOptions has values that were overridden explicitly on the command line,
            // then write the values from the command onto formattingOptions.
            OverwriteExplicitlySetCommandLineOptionsOnto(formattingOptions);

            // If the formattingOptions are incomplete, backfill from the command line,
            // to make the formatting options as complete as possible.  formattingOptions,
            // in the end, may still be incomplete after this, since the options on the command
            // line are nullable.  Mdfmt has default behavior for null/missing options, as documented
            // on the info displayed by mdfmt --help.
            if (!formattingOptions.IsComplete())
            {
                formattingOptions.PopulateFrom(_commandLineFormattingOptions);
            }

            return formattingOptions;
        }
        // In all other cases, simply return FormattingOptions based on the values passed to the
        // command line.
        else
        {
            return _commandLineFormattingOptions;
        }
    }

    /// <summary>
    /// In the instance of <see cref="FormattingOptions"/> passed in, replace property values
    /// by explicit overrides from the command line.
    /// </summary>
    /// <param name="formattingOptions">Instance of <see cref="FormattingOptions"/> to edit</param>
    private void OverwriteExplicitlySetCommandLineOptionsOnto(FormattingOptions formattingOptions)
    {
        if (ArgNames.Contains("-f") || ArgNames.Contains("--flavor"))
        {
            formattingOptions.Flavor = _commandLineFormattingOptions.Flavor;
        }
        if (ArgNames.Contains("-h") || ArgNames.Contains("--heading-numbers"))
        {
            formattingOptions.HeadingNumbering = _commandLineFormattingOptions.HeadingNumbering;
        }
        if (ArgNames.Contains("-t") || ArgNames.Contains("--toc-threshold"))
        {
            formattingOptions.TocThreshold = _commandLineFormattingOptions.TocThreshold;
        }
        if (ArgNames.Contains("--newline-strategy"))
        {
            formattingOptions.NewlineStrategy = _commandLineFormattingOptions.NewlineStrategy;
        }
    }
}
