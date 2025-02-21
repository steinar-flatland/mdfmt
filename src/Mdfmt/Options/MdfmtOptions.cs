using FluentValidation;
using System.Collections.Generic;

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
/// Full path defining the root of files that Mdfmt can see and process.  When there are
/// configuration files present, the directory in which these files exist defines the processing
/// root.  When there are no configuration files, the directory of the target path is used.
/// (The target path defines either the single Markdown file, or the directory, being processed
/// by the program.)
/// </param>
/// <param name="mdfmtConfigurationFilePaths">
/// A list of full paths of configuration file(s) to load, or null or empty list if there are no
/// configuration files to load.  This is for optionally loading <see cref="MdfmtProfile"/>.
/// </param>
internal class MdfmtOptions
    (
        string[] args,
        CommandLineOptions commandLineOptions,
        string processingRoot,
        IReadOnlyList<string> mdfmtConfigurationFilePaths
    )
{
    /// <summary>
    /// Used to validate <see cref="FormattingOptions"/> instance before returning it from the
    /// <c>GetFormattingOptions(string cpath)</c> method.
    /// </summary>
    private readonly FormattingOptionsValidator2 _formattingOptionsValidator = new();

    /// <summary>
    /// Names of arguments that were explicitly set on the command line.  This knowledge is used to
    /// give precedence to explicitly set command line arguments, overriding values from mdfmt
    /// configuration file.
    /// </summary>
    public IReadOnlySet<string> ArgNames { get; } = GetNamesOf(args);

    /// <summary>
    /// <see cref="CommandLineOptions"/> instance that was parsed from the raw command line arguments by the 
    /// <c>CommandLineParser</c> NuGet package.
    /// </summary>
    public CommandLineOptions CommandLineOptions { get; } = commandLineOptions;

    /// <summary>
    /// Full path defining the root of files that Mdfmt can see and process.  If there exists a configuration
    /// file for loading the <see cref="MdfmtProfile"/>, it is directly inside this same directory.
    /// </summary>
    public string ProcessingRoot { get; } = processingRoot;

    /// <summary>
    /// A list of full paths of configuration file(s) to load, or null or empty list if there are no
    /// configuration files to load.  This is for optionally loading the <see cref="MdfmtProfile"/>.
    /// </summary>
    public IReadOnlyList<string> MdfmtConfigurationFilePaths { get; } = mdfmtConfigurationFilePaths;

    /// <summary>
    /// A data structure with information loaded from configuration file(s) or null if there are no
    /// configuration files to load.
    /// </summary>
    public MdfmtProfile MdfmtProfile { get; } = MdfmtProfileLoader.Load(mdfmtConfigurationFilePaths);

    /// <summary>
    /// Instance of <see cref="FormattingOptions"/> instantiated based on command line options.
    /// This knowledge is used as a fallback mechanism:  Any formatting options not specified
    /// through the optional Mdfmt configuration file are taken from here as a fallback.
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
    /// determined through the mdfmt configuration file.
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
            LineNumberingThreshold = commandLineOptions.LineNumberingThreshold,
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
    /// Whether to perform a link audit.
    /// </summary>
    public bool AuditLinks => CommandLineOptions.AuditLinks;    

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
        // If there is an MdfmtProfile, and it maps the cpath to an instance of FormattingOptions, then return based on that.
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

            try
            {
                // Make sure that the assembled formatting options are valid.  This could happen if
                // there are inconsistencies involving more than one option, e.g. HeadingNumbering
                // option is specified without a Flavor, or TocThreshold >= 1 with no Flavor.
                _formattingOptionsValidator.ValidateAndThrow(formattingOptions);
            }
            catch (ValidationException ex)
            {
                Output.Error($"Formatting options for {cpath} are inconsistent: {ex.Message}.  Please check configuration files in {ProcessingRoot}.");
                throw new ExitException(ExitCodes.GeneralError);
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
        if (ArgNames.Contains("-l") || ArgNames.Contains("--line-numbering-threshold"))
        {
            formattingOptions.LineNumberingThreshold = _commandLineFormattingOptions.LineNumberingThreshold;
        }
        if (ArgNames.Contains("--newline-strategy"))
        {
            formattingOptions.NewlineStrategy = _commandLineFormattingOptions.NewlineStrategy;
        }
    }
}
