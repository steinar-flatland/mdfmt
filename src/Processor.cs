using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Loaders;
using Mdfmt.Model;
using Mdfmt.Options;
using Mdfmt.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mdfmt;

internal class Processor
{
    private readonly CommandLineOptions _options;
    private readonly List<string> _filePaths;
    private readonly PathUtils _pathUtils;
    private readonly RegionsLoader _regionsLoader;
    private readonly ILinkDestinationGenerator _linkDestinationGenerator;
    private readonly TocGenerator _tocGenerator;
    private readonly Updater _updater;
    private static readonly string MdWildcard = "*.md";
    private static readonly string Indent = "    ";

    public Processor(CommandLineOptions options)
    {
        _options = options;
        _filePaths = FindFilePathsToProcess();
        _pathUtils = new(options.Path);
        _regionsLoader = new(options.NewlineStrategy);
        _linkDestinationGenerator = LinkDestinationGeneratorFactory.Manufacture(_options.Platform);
        _tocGenerator = new(_linkDestinationGenerator);
        _updater = new(_tocGenerator, _options.MinimumEntryCount, _linkDestinationGenerator, _options.Verbose);
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
            Console.Write($"Processing {filePath}.");
        }

        string cpath = _pathUtils.MakeRelative(filePath);

        // Load Markdown file into Regions data structure.
        Regions regions = _regionsLoader.Load(filePath, cpath);

        // Make a Headings data structure based on regions.
        Headings headings = regions.Headings;

        if (_options.Verbose)
        {
            Console.WriteLine($" Loaded {regions.Count} region{(regions.Count != 1 ? 's' : string.Empty)} " +
                $"with {headings.Count} heading{(headings.Count != 1 ? 's' : string.Empty)}.");
        }

        // Update the regions, in reference to its headings.
        _updater.Update(regions, headings);

        // If the regions were modified, save the Markdown file.
        if (regions.Modified)
        {
            File.WriteAllText(filePath, regions.Content);
            if (_options.Verbose)
            {
                Console.WriteLine("  Wrote file");
            }
            else
            {
                Console.WriteLine(filePath);
            }
        }
    }
}
