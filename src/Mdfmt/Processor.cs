using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Loaders;
using Mdfmt.Model;
using Mdfmt.Options;
using Mdfmt.Updaters;
using Mdfmt.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mdfmt;

internal class Processor
{
    private readonly MdfmtOptions _options;
    private readonly List<string> _filePaths;
    private readonly MdStructLoader _mdStructLoader;
    private readonly Dictionary<Flavor, LinkUpdater> _linkUpdaters = [];
    private readonly Dictionary<Flavor, TocUpdater> _tocUpdaters = [];
    private readonly string MdWildcard = "*.md";

    public Processor(MdfmtOptions options)
    {
        _options = options;
        _filePaths = FindFilePathsToProcess();
        _mdStructLoader = new();

        foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            CreateUpdaters(flavor);
        }
    }

    private void CreateUpdaters(Flavor flavor)
    {
        ILinkDestinationGenerator linkDestinationGenerator =
            LinkDestinationGeneratorFactory.Manufacture(flavor);
        _linkUpdaters.Add(flavor, new LinkUpdater(linkDestinationGenerator));
        TocGenerator tocGenerator = new(linkDestinationGenerator);
        _tocUpdaters.Add(flavor, new TocUpdater(tocGenerator));
    }

    private List<string> FindFilePathsToProcess()
    {
        if (!Directory.Exists(_options.TargetPath))
        {
            return [_options.TargetPath];
        }
        else if (_options.Recursive)
        {
            return new List<string>(Directory.GetFiles(_options.TargetPath, MdWildcard, SearchOption.AllDirectories));
        }
        else
        {
            return new List<string>(Directory.GetFiles(_options.TargetPath, MdWildcard, SearchOption.TopDirectoryOnly));
        }
    }

    public void Run()
    {
        if (_options.Verbose)
        {
            Output.Info($"Mdfmt v{Program.Version}{Environment.NewLine}");
            Output.Info($"Current Working Directory: {Directory.GetCurrentDirectory()}{Environment.NewLine}");
            Output.Info($"{nameof(_options.CommandLineOptions)} {_options.CommandLineOptions}{Environment.NewLine}");
            Output.Info($"Absolute TargetPath: {Path.GetFullPath(_options.CommandLineOptions.TargetPath)}{Environment.NewLine}");
            Output.Info($"Explicitly Set Option Names:{Environment.NewLine}{(_options.ArgNames.Count == 0 ? "none" : string.Join(", ", _options.ArgNames))}{Environment.NewLine}");
            Output.Info($"Processing Root: {_options.ProcessingRoot}{Environment.NewLine}");
            Output.Info($"Mdfmt configuration file: {_options.MdfmtConfigurationFilePath ?? "none"}");
            if (_options.MdfmtProfile != null)
            {
                Output.Info(_options.MdfmtProfile);
                Output.Info($"Note: CpathToOptions keys are relative to Procesing Root.");
            }
        }

        foreach (string filePath in _filePaths)
        {
            Process(filePath);
        }
    }

    private void Process(string filePath)
    {
        string cpath = PathUtils.MakeRelative(_options.ProcessingRoot, filePath);
        FormattingOptions fpo = _options.GetFormattingOptions(cpath);
        string absolutePath = Path.GetFullPath(filePath);
        if (_options.Verbose)
        {
            Output.Info($"{Environment.NewLine}Processing {absolutePath}", true, ConsoleColor.Cyan);
            Output.Info($"{fpo.GetType().Name}:{Environment.NewLine}{fpo}");
        }

        // Load Markdown file into MdStruct data structure.
        MdStruct md = _mdStructLoader.Load(absolutePath, fpo.NewlineStrategy);

        if (_options.Verbose)
        {
            Output.Info($"Loaded {md.RegionCount} region{(md.RegionCount != 1 ? 's' : string.Empty)} " +
                $"with {md.HeadingCount} heading{(md.HeadingCount != 1 ? 's' : string.Empty)}.");
        }

        if (fpo.HeadingNumbering != null)
        {
            HeadingNumberUpdater.Update(md, fpo.HeadingNumbering, _options.Verbose);
        }

        if (fpo.TocThreshold is int tocThreshold)
        {
            if (tocThreshold == 0)
            {
                TocDeleter.DeleteToc(md, _options.Verbose);
            }
            else if (tocThreshold > 0)
            {
                // options validation has already assured non-null fpo.Flavor in this case.
                _tocUpdaters[(Flavor)fpo.Flavor].Update(md, tocThreshold, _options.Verbose);
            }
            else
            {
                throw new InvalidOperationException("Code maintenance error.  tocThreshold < 0 is an invalid state");
            }
        }

        if (fpo.Flavor is Flavor flavor)
        {
            // If options don't indicate anything about TOC threshold, but there's already a TOC
            // present, then we need to maintain it, making sure its up to date with the the flavor.
            if (fpo.TocThreshold == null && md.HasToc)
            {
                _tocUpdaters[flavor].Update(md, 1, _options.Verbose);
            }
            _linkUpdaters[flavor].Update(md, _options.Verbose);
        }

        if (fpo.LineNumberingThreshold is int lineNumberingThreshold)
        {
            LineNumberUpdater.Update(md, lineNumberingThreshold, _options.Verbose);
        }

        // If the MdStruct was modified, save the Markdown file.
        if (md.IsModified)
        {
            File.WriteAllText(absolutePath, md.Content);
            if (_options.Verbose)
            {
                Output.Emphasis($"Wrote file {absolutePath}");
            }
            else
            {
                Output.Emphasis(absolutePath);
            }
        }
    }
}
