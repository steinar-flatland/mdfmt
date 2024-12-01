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
    private readonly string Indent = "    ";

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
            Console.WriteLine("Command Line Options:");
            ObjectPrinter.PrintProperties(_options, Indent);
        }

        foreach (string filePath in _filePaths)
        {
            Process(filePath);
        }
    }

    private void Process(string filePath)
    {
        if (_options.Verbose)
        {
            Console.WriteLine();
            Console.WriteLine($"Processing {filePath}.");
        }

        // Retrieve FileProcessingOptions for the current filePath.
        string cpath = PathUtils.MakeRelative(_options.Path, filePath);
        FileProcessingOptions fpo = _options.GetFileProcessingOptions(cpath);
        if (_options.Verbose)
        {
            Console.WriteLine("FileProcessingOptions:");
            Console.WriteLine(fpo);
        }

        // Load Markdown file into MdStruct data structure.
        MdStruct md = _mdStructLoader.Load(filePath, (NewlineStrategy)fpo.NewlineStrategy);

        if (_options.Verbose)
        {
            Console.WriteLine($"Loaded {md.RegionCount} region{(md.RegionCount != 1 ? 's' : string.Empty)} " +
                $"with {md.HeadingCount} heading{(md.HeadingCount != 1 ? 's' : string.Empty)}.");
        }

        HeadingNumberUpdater.Update(md, fpo.HeadingNumbering);
        _tocUpdaters[(Flavor)fpo.Flavor].Update(md, (int)fpo.TocThreshold, _options.Verbose);
        _linkUpdaters[(Flavor)fpo.Flavor].Update(md, _options.Verbose);

        // If the MdStruct was modified, save the Markdown file.
        if (md.IsModified)
        {
            File.WriteAllText(filePath, md.Content);
            if (_options.Verbose)
            {
                Console.WriteLine("Wrote file");
            }
            else
            {
                Console.WriteLine(filePath);
            }
        }
    }
}
