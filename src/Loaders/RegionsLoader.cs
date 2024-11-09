using Mdfmt.Model;
using Mdfmt.Options;
using Mdfmt.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mdfmt.Loaders;

//TODO: It would be nice to share base parsing state management between this class and HeadingsLoader.

/// <summary>
/// Loads a single Markdown file into a Regions data structure.  It is safe to reuse the same
/// instance of this class to load a series of files by repeatedly calling the Load() method.
/// </summary>
internal class RegionsLoader(NewlineStrategy newlineStrategy)
{
    /// <summary>
    /// Option for how to manage newlines when modified Markdown files are written.
    /// (This state does not need resetting between calls to Load().)
    /// </summary>
    private readonly NewlineStrategy _newlineStrategy = newlineStrategy;

    // Goal of the Load() method is to populate and return this inside of a Regions object.
    private List<Region> _regions;

    /// <summary>
    /// State that controls the behavior of the parser.
    /// </summary>
    private ParserState _state;

    /// <summary>
    /// Used to build up state of an atomic region, when the region spans multiple lines.
    /// </summary>
    private readonly StringBuilder _atomicRegionContent = new();

    /// <summary>
    /// The first time a table of contents is parsed during a call to Load(), remembmer its Region here.
    /// </summary>
    private Region _firstToc;

    /// <summary>
    /// The first time a heading is parsed during a call to Load(), rembmer its index here.
    /// </summary>
    private int _firstHeadingIndex;

    /// <summary>
    /// If the loader needs to build a line break into a region, the specific newline character or
    /// character sequence to use.
    /// </summary>
    private string _newlineSequence;

    /// <summary>
    /// Reset the loader at start of load.  This makes it safe to call the Load() method repeatedly
    /// on the same instance of this class.
    /// </summary>
    private void Reset(string newlineSequence)
    {
        // When maintaining this method, consider all state of this class and whether it needs to be reset.
        _regions = [];
        _state = ParserState.Normal;
        _atomicRegionContent.Clear();
        _firstToc = null;
        _firstHeadingIndex = -1;
        _newlineSequence = newlineSequence;
    }

    /// <summary>
    /// Load a single Markdown file into a Regions data structure.
    /// </summary>
    /// <param name="filePath">File path of the Markdown file to load.</param>
    /// <param name="cpath">Canonical relative file path, or cpath.</param>
    /// <returns>Regions</returns>
    /// </summary>
    public Regions Load(string filePath, string cpath)
    {
        // Load the content to parse, and use this content in combination with the user's preferred
        // newline strategy to calculate the newline to use.
        string fileContent = File.ReadAllText(filePath);
        string newlineSequence = Newline.NewlineSequence(_newlineStrategy, fileContent, out bool modified);

        // Reset the parser, making it ready for a new run.
        Reset(newlineSequence);

        //Note: I tried reading the file one line at a time with StreamReader, which is more memory
        // efficient. However, there is a limitation where StreamReader is unable to see a blank
        // line at end of file. Using File.ReadAllText avoids this problem.  It is convenient to read
        // all the content up front anyway, for calculating the newline sequence to use.
        // -SF 11/3/2024

        // Break the content into lines, for line-by-line processing.  Each string line DOES NOT
        // have any newline characters at the end.
        string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        fileContent = null; // done with this large string

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            switch (_state)
            {
                case ParserState.Normal:
                    {
                        // If start of TOC marker, transition to ParserState.InTableOfContents.
                        if (trimmedLine.StartsWith(Markers.BeginTocMarker))
                        {
                            SaveLine(line);
                            _state = ParserState.InTableOfContents;
                            continue;
                        }

                        // If start of fence marker, transition to ParserState.InFencedCodeBlock.
                        if (trimmedLine.StartsWith(Markers.FenceMarker))
                        {
                            _regions.Add(new AtomicRegion(line, false));
                            _state = ParserState.InFencedCodeBlock;
                            continue;
                        }

                        // This is a line of indented code.
                        if (line.StartsWith(Markers.FourSpaces) || line.StartsWith('\t'))
                        {
                            _regions.Add(new AtomicRegion(line, false));
                            continue;
                        }

                        // The line is "normal". Basically, its normal paragraph and heading type
                        // content, possibly with HTML comments mixed in.
                        NormalLineParseResult parseResult = NormalLineParser.Parse(false, line);
                        SaveParseResultAsRegion(parseResult);

                        // If in an HTML comment at end of line, transition to ParserState.InHtmlComment. 
                        if (parseResult.InHtmlComment)
                        {
                            _state = ParserState.InHtmlComment;
                            continue;
                        }
                    }
                    break;

                case ParserState.InTableOfContents:
                    SaveLine(line);
                    // If end of TOC marker, transition back to ParserState.Normal.
                    if (trimmedLine.StartsWith(Markers.EndTocMarker))
                    {
                        AtomicRegion toc = new(_atomicRegionContent.ToString(), isMutable: true, isHeading: false, isToc: true);
                        _regions.Add(toc);
                        _firstToc ??= toc;
                        _atomicRegionContent.Clear();
                        _state = ParserState.Normal;
                        continue;
                    }
                    break;

                case ParserState.InFencedCodeBlock:
                    _regions.Add(new AtomicRegion(line, false));
                    // If end of fence marker, transition back to ParserState.Normal.
                    if (trimmedLine.StartsWith(Markers.FenceMarker))
                    {
                        _state = ParserState.Normal;
                        continue;
                    }
                    break;

                case ParserState.InHtmlComment:
                    {
                        NormalLineParseResult parseResult = NormalLineParser.Parse(true, line);
                        SaveParseResultAsRegion(parseResult);

                        // If not in an HTML comment at end of line, transition back to ParserState.Normal. 
                        if (!parseResult.InHtmlComment)
                        {
                            _state = ParserState.Normal;
                            continue;
                        }
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unhandled {nameof(ParserState)}: {_state}");
            }
        }
        return new Regions(_regions, filePath, cpath, newlineSequence, _firstToc, _firstHeadingIndex, modified);
    }

    private void SaveLine(string line)
    {
        if (_atomicRegionContent.Length > 0)
            _atomicRegionContent.Append(_newlineSequence);
        _atomicRegionContent.Append(line);
    }

    private void SaveParseResultAsRegion(NormalLineParseResult parseResult)
    {
        // Depending on the state of the parseResult, save either a single atomic region or a single composite region.
        if (parseResult.AtomicRegions.Count == 1)
            _regions.Add(parseResult.AtomicRegions[0]);
        else if (parseResult.AtomicRegions.Count > 1)
            _regions.Add(new CompositeRegion(parseResult.AtomicRegions, parseResult.IsHeading));
        else
            throw new InvalidOperationException($"Bug!  Invariant that class {nameof(NormalLineParseResult)} always have at least one {nameof(AtomicRegion)} violated.");

        // If this is the first heading during the parse, save its index.
        if (_firstHeadingIndex == -1 && parseResult.IsHeading)
        {
            _firstHeadingIndex = _regions.Count - 1;
        }
    }
}
