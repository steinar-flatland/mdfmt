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
        if (!Directory.Exists(_options.Path))
        {
            return [_options.Path];
        }
        else if (_options.Recursive)
        {
            return new List<string>(Directory.GetFiles(_options.Path, MdWildcard, SearchOption.AllDirectories));
        }
        else
        {
            return new List<string>(Directory.GetFiles(_options.Path, MdWildcard, SearchOption.TopDirectoryOnly));
        }
    }

    public void Run()
    {
        if (_options.Verbose)
        {
            Output.Info(_options);
        }

        foreach (string filePath in _filePaths)
        {
            Process(filePath);
        }
    }

    private void Process(string filePath)
    {
        string cpath = PathUtils.MakeRelative(_options.Path, filePath);
        FileProcessingOptions fpo = _options.GetFileProcessingOptions(cpath);
        if (_options.Verbose)
        {
            Output.Info($"{Environment.NewLine}Processing {filePath}", true, ConsoleColor.Cyan);
            Output.Info($"{fpo.GetType().Name}:{Environment.NewLine}{fpo}");
        }

        // Load Markdown file into MdStruct data structure.
        MdStruct md = _mdStructLoader.Load(filePath, fpo.NewlineStrategy);

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

        // If the MdStruct was modified, save the Markdown file.
        if (md.IsModified)
        {
            File.WriteAllText(filePath, md.Content);
            if (_options.Verbose)
            {
                Output.Emphasis($"Wrote file {filePath}");
            }
            else
            {
                Output.Emphasis(filePath);
            }
        }
    }
}
