using System.Collections.Generic;
using System.IO;

namespace Mdfmt.Options;

/// <summary>
/// <para>
/// Find optional config files that provide instructions to Mdfmt.  (When configuration files are
/// absent, only the command line tells Mdfmt what to do. When configuration files are present,
/// formatting from the command line takes priority over formatting from configuration files.)
/// </para>
/// <para>
/// To use this class:  Instantiate it with the command line options passed to the program and the
/// <c>targetPathIsDirectory</c> flag.  Then, read the <c>ProcessingRoot</c> property to know the
/// directory containing config files, and read the <c>MdfmtConfigurationFilePaths</c> property to
/// know the full paths of all configuration files, in the order they should be loaded.
/// </para>
/// </summary>
internal class ConfigFilesFinder
{
    /// <summary>
    /// Command line options passed to the program.
    /// </summary>
    private CommandLineOptions CommandLineOptions { get; }

    /// <summary>
    /// If Mdfmt is processing a single file, this is the path of the directory containing the file,
    /// and if Mdfmt is processing the Markdown files in a directory, this is the path of that directory.
    /// </summary>
    private string TargetDirectory { get; }

    /// <summary>
    /// The processing root path, which defines the scope of files Mdfmt can see.  Any configuration 
    /// files are directly in this directory.
    /// </summary>
    public string ProcessingRoot { get; internal set; }

    /// <summary>
    /// Backing storage for the <c>MdfmtConfigurationFilePaths</c> property.
    /// </summary>
    private readonly List<string> _mdfmtConfigurationFilePaths = [];

    /// <summary>
    /// A list of full paths of configuration file(s) to load, or null or empty list if there are no
    /// configuration files to load.
    /// </summary>
    public IReadOnlyList<string> MdfmtConfigurationFilePaths => _mdfmtConfigurationFilePaths;

    /// <summary>
    /// Constructor of class <c>ConfigFilesFinder</c>.  Throws <see cref="ExitException"/> if an
    /// environment was specified on the command line, and the configuration file for this
    /// environment is not found.
    /// </summary>
    /// <param name="commandLineOptions">
    /// Command line options provided to the program.
    /// </param>
    /// <param name="targetPathIsDirectory">
    /// <c>true</c> if the target path Mdfmt is being asked to process is a directory, and <c>false</c>
    /// if it is an individual file.
    /// </param>
    /// <exception cref="ExitException"/>
    public ConfigFilesFinder(CommandLineOptions commandLineOptions, bool targetPathIsDirectory)
    {
        CommandLineOptions = commandLineOptions;
        TargetDirectory = targetPathIsDirectory ? commandLineOptions.TargetPath : Path.GetDirectoryName(commandLineOptions.TargetPath);
        FinishInitializing();
    }

    /// <summary>
    /// Set up the <c>ProcessingRoot</c> and <c>MdfmtConfigurationFilePaths</c> properties.
    /// Throws an <see cref="ExitException"/> if an environment was specified on the command
    /// line, and the configuration file for this environment is not found.
    /// </summary>
    /// <exception cref="ExitException"/>
    private void FinishInitializing()
    {
        bool lookingForEnvironmentSpecificFile = !string.IsNullOrEmpty(CommandLineOptions.Environment);

        // Foreach directory starting with the TargetDirectory and then each ancestor directory...
        DirectoryInfo directoryInfo = new(TargetDirectory);
        do
        {
            string candidateProcessingRoot = directoryInfo.FullName;
            string dotMdfmtFile = Path.Combine(candidateProcessingRoot, ".mdfmt");
            string mdfmtJsonFile = Path.Combine(candidateProcessingRoot, "mdfmt.json");
            string envSpecificFile = lookingForEnvironmentSpecificFile ?
                Path.Combine(candidateProcessingRoot, $"mdfmt.{CommandLineOptions.Environment}.json") :
                null;
            bool dotMdfmtFileExists = File.Exists(dotMdfmtFile);
            bool mdfmtJsonFileExists = File.Exists(mdfmtJsonFile);
            bool envSpecificFileExists = lookingForEnvironmentSpecificFile && File.Exists(envSpecificFile);
            if (dotMdfmtFileExists || mdfmtJsonFileExists || envSpecificFileExists)
            {
                // Processing root found, based on presence of configuration.
                ProcessingRoot = candidateProcessingRoot;

                // First add file containing base configuration, if any, to MdfmtConfigurationFilePaths.
                if (dotMdfmtFileExists && mdfmtJsonFileExists)
                {
                    Output.Warn($"Ignoring {dotMdfmtFile} and using {mdfmtJsonFile}.");
                    _mdfmtConfigurationFilePaths.Add(mdfmtJsonFile);
                }
                else if (dotMdfmtFileExists)
                {
                    _mdfmtConfigurationFilePaths.Add(dotMdfmtFile);
                }
                else if (mdfmtJsonFileExists)
                {
                    _mdfmtConfigurationFilePaths.Add(mdfmtJsonFile);
                }

                // If looking for an environment-specific file (because an environment was specified
                // on the command line) then either add it to MdfmtConfigurationFilePaths or throw
                // if not found.
                if (lookingForEnvironmentSpecificFile)
                {
                    if (envSpecificFileExists)
                    {
                        _mdfmtConfigurationFilePaths.Add(envSpecificFile);
                    }
                    else
                    {
                        Output.Error($"Environment \"{CommandLineOptions.Environment}\" was specified on the command line, but file {envSpecificFile} was not found.");
                        throw new ExitException(ExitCodes.MisuseOfCommand);
                    }
                }

                // Initialization complete.
                return;
            }
        } while ((directoryInfo = directoryInfo.Parent) != null);

        // Since we scanned all the way to the root of the file system without finding configuration
        // files, then use the target directory as the processing root.
        ProcessingRoot = new DirectoryInfo(TargetDirectory).FullName;

        if (lookingForEnvironmentSpecificFile)
        {
            // If we get here, no config files were found.  So if we were looking for an environment-specific
            // configuration file, that's a problem.
            Output.Error($"Environment \"{CommandLineOptions.Environment}\" was specified on the command line but no mdfmt.{CommandLineOptions.Environment}.json file was found, neither in {ProcessingRoot} nor in an ancestor directory.");
            throw new ExitException(ExitCodes.MisuseOfCommand);
        }
    }
}
