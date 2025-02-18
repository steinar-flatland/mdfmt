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
    private readonly MdStructLoader _mdStructLoader;
    private readonly Dictionary<Flavor, InDocumentLinkUpdater> _inDocumentLinkUpdaters = [];
    private readonly Dictionary<Flavor, CrossDocumentLinkUpdater> _crossDocumentLinkUpdaters = [];
    private readonly Dictionary<Flavor, TocUpdater> _tocUpdaters = [];
    private readonly HeadingNumberUpdater _headingNumberUpdater;

    /// <summary>
    /// Dictionary keyed on a string of the form <c>$"{cpath}{fragment}"</c>.  <c>{cpath}</c> is the
    /// canonical relative path, relative to the processing root, of a Markdown file.  <c>{fragment}</c>
    /// is a <c>'#'</c> character followed by slugified heading text.  The key is stale, i.e. it is
    /// for a heading that has changed because its heading number (which is part of the heading) 
    /// changed, e.g. the heading number could have been removed when it was present, added when it
    /// was missing, or modified if it already existed and then the number changed during heading renumbering.
    /// Each key, with stale information, maps to the new updated heading text.  The purpose of this
    /// data structure is to drive cross-document link updating.
    /// </summary>
    private readonly Dictionary<string, string> _staleCpathFragmentToUpdatedHeadingText = [];

    /// <summary>
    /// This is a set of keys like the ones that occur in the <c>_staleCpathFragmentToUpdatedHeadingText</c>
    /// dictionary, but the keys cannot be included in that dictionary because documents had duplicate headings
    /// (a Markdown anti-practice) and that would have led to key collisions in
    /// <c>_staleCpathFragmentToUpdatedHeadingText</c>.  The purpose of saving this information is to 
    /// generate warning messages during cross-document link updating, to inform the user of things they
    /// must investigate manually.  With well-structured Markdown, this kind of ambiguity won't occur,
    /// but we have to be prepared for it.
    /// </summary>
    private readonly HashSet<string> _ambiguousStaleCpathFragments = [];

    /// <summary>
    /// One of each different kind of link destination generator.
    /// </summary>
    private readonly List<ILinkDestinationGenerator> _linkDestinationGenerators = LinkDestinationGeneratorFactory.ManufactureOneOfEach();

    private readonly string MdWildcard = "*.md";

    public Processor(MdfmtOptions options)
    {
        _options = options;
        _mdStructLoader = new();

        foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            CreateUpdaters(flavor);
        }

        _headingNumberUpdater = new();
    }

    private void CreateUpdaters(Flavor flavor)
    {
        ILinkDestinationGenerator linkDestinationGenerator =
            LinkDestinationGeneratorFactory.Manufacture(flavor);
        _inDocumentLinkUpdaters.Add(flavor, new InDocumentLinkUpdater(linkDestinationGenerator));
        _crossDocumentLinkUpdaters.Add(flavor, new CrossDocumentLinkUpdater(linkDestinationGenerator, _staleCpathFragmentToUpdatedHeadingText, _ambiguousStaleCpathFragments));
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

            string loadedFromInfo;
            if (_options.MdfmtConfigurationFilePaths == null || _options.MdfmtConfigurationFilePaths.Count == 0)
            {
                loadedFromInfo = ": none";
            }
            else if (_options.MdfmtConfigurationFilePaths.Count == 1)
            {
                loadedFromInfo = $": {_options.MdfmtConfigurationFilePaths[0]}";
            }
            else
            {
                loadedFromInfo = $" loaded from the following files:";
            }
            Output.Info($"Mdfmt configuration file{loadedFromInfo}");

            if (_options.MdfmtConfigurationFilePaths.Count > 1)
            {
                foreach (string filePath in _options.MdfmtConfigurationFilePaths)
                {
                    Output.Info($"  - {filePath}");
                }
            }

            if (_options.MdfmtProfile != null)
            {
                Output.Info(_options.MdfmtProfile);
                Output.Info($"Note: CpathToOptions keys are relative to Procesing Root.");
            }
        }

        // Apply formatting to files in the target path.
        List<string> filePaths = FindFilePathsToProcess();
        foreach (string filePath in filePaths)
        {
            ApplyFormatting(filePath);
        }

        UpdateCrossDocumentLinks();
    }

    private void ApplyFormatting(string filePath)
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
        MdStruct md = _mdStructLoader.Load(absolutePath, cpath, fpo.NewlineStrategy);

        if (_options.Verbose)
        {
            Output.Info($"Loaded {md.RegionCount} region{(md.RegionCount != 1 ? 's' : string.Empty)} " +
                $"with {md.HeadingCount} heading{(md.HeadingCount != 1 ? 's' : string.Empty)}.");
        }

        if (fpo.HeadingNumbering != null)
        {
            _headingNumberUpdater.Update(md, fpo.HeadingNumbering, _options.Verbose);
            PrepareForCrossDocumentLinkUpdating(cpath);
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
            _inDocumentLinkUpdaters[flavor].Update(md, _options.Verbose);
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
                Output.Emphasis($"- Wrote file {absolutePath}");
            }
            else
            {
                Output.Emphasis(absolutePath);
            }
        }
    }

    /// <summary>
    /// After each use of the <c>_headingNumberUpdater</c> to ensure that heading numbers are up
    /// to date within a Markdown document, call this method to save information in 
    /// <c>_staleCpathFragmentToUpdatedHeadingText</c> and in <c>_ambiguousStaleCpathFragments</c>,
    /// which builds up the information necessary to drive cross-document link updating.
    /// </summary>
    /// <param name="cpath"></param>
    private void PrepareForCrossDocumentLinkUpdating(string cpath)
    {
        foreach (string oldHeading in _headingNumberUpdater.OldToNewHeadingMap.Keys)
        {
            string newHeading = _headingNumberUpdater.OldToNewHeadingMap[oldHeading];
            foreach (ILinkDestinationGenerator linkDestinationGenerator in _linkDestinationGenerators)
            {
                string key = linkDestinationGenerator.GenerateCrossDocumentLinkDestination(cpath, oldHeading);
                _staleCpathFragmentToUpdatedHeadingText[key] = newHeading;
            }
        }
        foreach (string duplicateHeading in _headingNumberUpdater.DuplicateHeadings)
        {
            foreach (ILinkDestinationGenerator linkDestinationGenerator in _linkDestinationGenerators)
            {
                string key = linkDestinationGenerator.GenerateCrossDocumentLinkDestination(cpath, duplicateHeading);
                _ambiguousStaleCpathFragments.Add(key);
            }
        }
    }

    private void UpdateCrossDocumentLinks()
    {
        if (_staleCpathFragmentToUpdatedHeadingText.Count == 0 && _ambiguousStaleCpathFragments.Count == 0)
        {
            if (_options.Verbose)
            {
                Output.Info($"{Environment.NewLine}Cross-document link updates not required.");
                return;
            }
        }
        if (_options.Verbose)
        {
            Output.Info($"{Environment.NewLine}Headings changed.  Scanning *.md files under {_options.ProcessingRoot} for cross-document link updates:{Environment.NewLine}");
        }
        List<string> filePaths = new(Directory.GetFiles(_options.ProcessingRoot, MdWildcard, SearchOption.AllDirectories));
        foreach (string filePath in filePaths)
        {
            UpdateCrossDocumentLinks(filePath);
        }
    }

    private void UpdateCrossDocumentLinks(string filePath)
    {
        string cpath = PathUtils.MakeRelative(_options.ProcessingRoot, filePath);
        FormattingOptions fpo = _options.GetFormattingOptions(cpath);
        string absolutePath = Path.GetFullPath(filePath);

        // Load Markdown file into MdStruct data structure.
        MdStruct md = _mdStructLoader.Load(absolutePath, cpath, fpo.NewlineStrategy);

        if (fpo.Flavor is Flavor flavor)
        {
            _crossDocumentLinkUpdaters[flavor].Update(md, _options.Verbose);
        }

        // If the MdStruct was modified, save the Markdown file.
        if (md.IsModified)
        {
            File.WriteAllText(absolutePath, md.Content);
            if (_options.Verbose)
            {
                Output.Emphasis($"- Wrote file {absolutePath}");
            }
            else
            {
                Output.Emphasis(absolutePath);
            }
        }
    }

}
