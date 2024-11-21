﻿using Mdfmt.Generators.Links;
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

public class Processor
{
    private readonly CommandLineOptions _options;
    private readonly List<string> _filePaths;
    private readonly MdStructLoader _mdStructLoader;
    private readonly Dictionary<Platform, LinkUpdater> _linkUpdaters = [];
    private readonly Dictionary<Platform, TocUpdater> _tocUpdaters = [];
    private readonly string MdWildcard = "*.md";
    private readonly string Indent = "    ";

    public Processor(CommandLineOptions options)
    {
        _options = options;
        _filePaths = FindFilePathsToProcess();
        _mdStructLoader = new();

        foreach (Platform platform in Enum.GetValues(typeof(Platform)))
        {
            CreateUpdaters(platform);
        }
    }

    private void CreateUpdaters(Platform platform)
    {
        ILinkDestinationGenerator linkDestinationGenerator =
            LinkDestinationGeneratorFactory.Manufacture(platform);
        _linkUpdaters.Add(platform, new LinkUpdater(linkDestinationGenerator));
        TocGenerator tocGenerator = new(linkDestinationGenerator);
        _tocUpdaters.Add(platform, new TocUpdater(tocGenerator));
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
        MdStruct md = _mdStructLoader.Load(filePath, _options.NewlineStrategy);

        if (_options.Verbose)
        {
            Console.WriteLine($" Loaded {md.RegionCount} region{(md.RegionCount != 1 ? 's' : string.Empty)} " +
                $"with {md.HeadingCount} heading{(md.HeadingCount != 1 ? 's' : string.Empty)}.");
        }

        HeadingNumberUpdater.Update(md, _options.HeadingNumbering);
        _tocUpdaters[_options.Platform].Update(md, _options.MinimumEntryCount, _options.Verbose);
        _linkUpdaters[_options.Platform].Update(md, _options.Verbose);

        // If the MdStruct was modified, save the Markdown file.
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
