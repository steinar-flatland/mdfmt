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

public class Processor
{
    private readonly CommandLineOptions _options;
    private readonly List<string> _filePaths;
    private readonly MdStructLoader _mdStructLoader;
    private readonly ILinkDestinationGenerator _linkDestinationGenerator;
    private readonly TocGenerator _tocGenerator;
    private readonly Updater _updater;
    private static readonly string MdWildcard = "*.md";
    private static readonly string Indent = "    ";

    public Processor(CommandLineOptions options)
    {
        _options = options;
        _filePaths = FindFilePathsToProcess();
        _mdStructLoader = new(options.NewlineStrategy);
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

        // Load Markdown file into MdStruct data structure.
        MdStruct md = _mdStructLoader.Load(filePath);

        if (_options.Verbose)
        {
            Console.WriteLine($" Loaded {md.RegionCount} region{(md.RegionCount != 1 ? 's' : string.Empty)} " +
                $"with {md.HeadingCount} heading{(md.HeadingCount != 1 ? 's' : string.Empty)}.");
        }

        // Update the regions, in reference to its headings.
        _updater.Update(md);

        // If the regions were modified, save the Markdown file.
        if (md.IsModified)
        {
            File.WriteAllText(filePath, md.Content);
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
