using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Loaders;
using Mdfmt.Model;
using Mdfmt.Options;
using Mdfmt.Updaters;
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
    private readonly IReadOnlyList<ILinkDestinationGenerator> _linkDestinationGenerators;
    private readonly LinkAuditor _linkAuditor;

    /// <summary>
    /// Dictionary keyed on a string of the form <c>$"{cpath}{fragment}"</c>.  <c>{cpath}</c> is the
    /// canonical relative path, relative to the processing root, of a Markdown file.  <c>{fragment}</c>
    /// is a <c>'#'</c> character followed by slugified heading text.  The key is stale, i.e. it is
    /// for a heading that has changed because its heading number (which is part of the heading) 
    /// changed, e.g. the heading number could have been removed when it was present, added when it
    /// was missing, or modified if it already existed.  Each key, with stale information, maps to 
    /// the new updated heading text.  The purpose of this data structure is to drive cross-document
    /// link updating.
    /// </summary>
    private readonly Dictionary<string, string> _staleCpathFragmentToUpdatedHeadingText = new([], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// This is a set of keys like the ones that occur in the <c>_staleCpathFragmentToUpdatedHeadingText</c>
    /// dictionary, but the keys cannot be included in that dictionary because documents had duplicate headings
    /// (a Markdown anti-practice) and that would have led to key collisions in
    /// <c>_staleCpathFragmentToUpdatedHeadingText</c>.  The purpose of saving this information is to 
    /// generate warning messages during cross-document link updating, to inform the user of things they
    /// must investigate manually.  With well-structured Markdown, this kind of ambiguity won't occur,
    /// but we have to be prepared for it.
    /// </summary>
    private readonly HashSet<string> _ambiguousStaleCpathFragments = new([], StringComparer.OrdinalIgnoreCase);

    public Processor(MdfmtOptions options)
    {
        _options = options;
        _mdStructLoader = new(_options.ProcessingRoot);
        _linkDestinationGenerators = LinkDestinationGeneratorFactory.ManufactureOneOfEach();

        Dictionary<string, string> fragmentToHeadingText = options.FlavorXdoc ? LoadFragmentToHeadingText() : null;
        foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            CreateUpdaters(flavor, fragmentToHeadingText);
        }

        _headingNumberUpdater = new();
        _linkAuditor = new(options, _mdStructLoader, _linkDestinationGenerators);
    }

    /// <summary>
    /// Load a dictionary of heading slug to the heading text from which the slug was generated,
    /// for all headings in all Markdown files within the processing root, for all ways of 
    /// slugifying.
    /// </summary>
    /// <returns>
    /// Dictionary of heading slug to heading.
    /// </returns>
    private Dictionary<string, string> LoadFragmentToHeadingText()
    {
        Dictionary<string, string> fragmentToHeadingText = [];
        foreach (string filePath in _options.AllMarkdownFilePaths)
        {
            MdStruct md = _mdStructLoader.Load(filePath);
            foreach (HeadingRegion headingRegion in md.HeadingRegions)
            {
                string headingText = headingRegion.HeadingText;
                foreach (ILinkDestinationGenerator linkDestinationGenerator in _linkDestinationGenerators)
                {
                    fragmentToHeadingText[$"#{linkDestinationGenerator.SlugifyHeadingText(headingText)}"] = headingText;
                }
            }
        }
        return fragmentToHeadingText;
    }

    private void CreateUpdaters(Flavor flavor, Dictionary<string, string> fragmentToHeadingText)
    {
        ILinkDestinationGenerator linkDestinationGenerator = LinkDestinationGeneratorFactory.Manufacture(flavor);
        _inDocumentLinkUpdaters.Add(flavor, new InDocumentLinkUpdater(linkDestinationGenerator));
        _crossDocumentLinkUpdaters.Add(flavor, new CrossDocumentLinkUpdater(linkDestinationGenerator, _staleCpathFragmentToUpdatedHeadingText, _ambiguousStaleCpathFragments, fragmentToHeadingText));
        TocGenerator tocGenerator = new(linkDestinationGenerator);
        _tocUpdaters.Add(flavor, new TocUpdater(tocGenerator));
    }

    public void Run()
    {
        if (_options.Verbose)
        {
            ShowVerboseOutput();
        }
        if (_options.AuditLinks)
        {
            _linkAuditor.AuditLinks();
        }
        else
        {
            ApplyFormatting();
        }
    }

    private void ShowVerboseOutput()
    {
        Output.Info($"Mdfmt v{Program.Version}{Environment.NewLine}");
        Output.Info($"Current Working Directory: {Directory.GetCurrentDirectory()}{Environment.NewLine}");
        Output.Info($"{nameof(_options.CommandLineOptions)} {_options.CommandLineOptions}{Environment.NewLine}");
        Output.Info($"Absolute TargetPath: {Path.GetFullPath(_options.TargetPath)}{Environment.NewLine}");
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

    private void ApplyFormatting()
    {
        // Apply formatting to files in the target path.
        foreach (string filePath in _options.MarkdownFilePaths)
        {
            ApplyFormatting(filePath);
        }

        UpdateCrossDocumentLinks();
    }

    private void ApplyFormatting(string filePath)
    {
        FormattingOptions fpo = _options.GetFormattingOptions(filePath);

        if (_options.Verbose)
        {
            Output.Info($"{Environment.NewLine}Processing {Path.GetFullPath(filePath)}", true, ConsoleColor.Cyan);
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
            _headingNumberUpdater.Update(md, fpo.HeadingNumbering, _options.Verbose);
            PrepareForCrossDocumentLinkUpdating(md.Cpath);
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
            File.WriteAllText(md.FilePath, md.Content);
            if (_options.Verbose)
            {
                Output.Emphasis($"- Wrote file {md.FilePath}");
            }
            else
            {
                Output.Emphasis(md.FilePath);
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

    /// <summary>
    /// Post-processing phase that always gets called at the end of <c>ApplyFormatting()</c>, to 
    /// ensure that cross-document links are up to date after other formatting changes have been made.
    /// </summary>
    private void UpdateCrossDocumentLinks()
    {
        // Cross-document link updating is required if --flavor-xdoc is set (user requested to verify flavor of cross-document links) or if some heading numbers changed.
        bool crossDocumentLinkUpdatingRequired = (_options.FlavorXdoc || _staleCpathFragmentToUpdatedHeadingText.Count > 0 || _ambiguousStaleCpathFragments.Count > 0);
        if (!crossDocumentLinkUpdatingRequired)
        {
            return;
        }

        if (_options.Verbose)
        {
            Output.Info($"{Environment.NewLine}Updating cross-document links");
        }

        foreach (string filePath in _options.AllMarkdownFilePaths)
        {
            // Determine formatting options to apply to the current filePath.
            FormattingOptions fpo = _options.GetFormattingOptions(filePath);

            // Warn on inconsistent options affecting the current filePath that create a risk of not
            // maintaining cross-document links.
            if (_options.FlavorXdoc && fpo.Flavor == null)
            {
                Output.Warn($"{Path.GetFullPath(filePath)}: Unknown flavor.  Unable to check and adjust flavor of cross-document links.");
            }

            // Continue unless there is something for the cross-document link updater to do.  It 
            // can't do anything if it does not know what flavor to target.
            if (fpo.Flavor == null)
            {
                continue;
            }

            // Load Markdown file into MdStruct data structure.
            MdStruct md = _mdStructLoader.Load(filePath, fpo.NewlineStrategy);

            // Invoke the right cross-document link updator for the flavor.
            _crossDocumentLinkUpdaters[fpo.Flavor.Value].Update(md, _options.Verbose);

            // If the MdStruct was modified, save the Markdown file.
            if (md.IsModified)
            {
                File.WriteAllText(md.FilePath, md.Content);
                if (_options.Verbose)
                {
                    Output.Emphasis($"- Wrote file {md.FilePath}");
                }
                else
                {
                    Output.Emphasis(md.FilePath);
                }
            }
        }
    }
}
