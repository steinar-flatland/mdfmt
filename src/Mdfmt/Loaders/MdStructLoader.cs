using Mdfmt.Model;
using Mdfmt.Options;
using Mdfmt.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mdfmt.Loaders;

/// <summary>
/// Loads a single Markdown file into a MdStruct data structure.  This class is implemented in a reusable way,
/// i.e., it is safe to call the <c>Load()</c> method multiple times on one instance of this class.
/// </summary>
public class MdStructLoader
{
    #region MdStructLoader_States

    /// <summary>
    /// Initial state of parser. In this state it processes lines with headings, paragraphs, etc.
    /// or it switches to one of the other states when special content is seen.
    /// </summary>
    private const int NormalState = 1;

    /// <summary>
    /// Table of contents that Mdfmt previously added, enclosed by <!--BEGIN_TOC--> and <!--END_TOC-->.
    /// </summary>
    private const int InTableOfContentsState = 2;

    /// <summary>
    /// Fenced code block enclosed in starting and ending ```.
    /// </summary>
    private const int InFencedCodeBlockState = 3;

    /// <summary>
    /// Inside a multi-line HTML comment.
    /// </summary>
    private const int InHtmlCommentState = 4;

    #endregion MdStructLoader_States

    #region Resettable_State

    /// <summary>
    /// Newline region to use for line breaks.
    /// </summary>
    private NewlineRegion _newlineRegion;

    /// <summary>
    /// Parser for a line of Markdown that is not code or a TOC.
    /// </summary>
    private LineParser _lineParser;

    /// <summary>
    /// A sequence of content regions parsed from the Markdown file.
    /// </summary>
    private List<Region> _regions;

    /// <summary>
    /// State that controls the behavior of the parser.
    /// </summary>
    private int _state;

    /// <summary>
    /// Used to build up state of a region, when the region spans multiple lines.
    /// </summary>
    private readonly StringBuilder _regionContent = new();

    /// <summary>
    /// Reset the loader at start of load.  This makes it safe to call the Load() method repeatedly
    /// on the same instance of this class.
    /// </summary>
    /// <param name="newlineRegion">
    /// Newline region to use for line breaks.
    /// </param>
    private void Reset(NewlineRegion newlineRegion)
    {
        _newlineRegion = newlineRegion;
        _lineParser = new();
        _regions = [];
        _state = NormalState;
        _regionContent.Clear();
    }

    #endregion Resettable_State

    /// <summary>
    /// <para>
    /// Load a single Markdown file into an MdStruct data structure.  It is safe to call
    /// <c>Load()</c> multiple times.
    /// </para>
    /// <para>
    /// Throws <c>NotImplementedException</c> if parser enters an unhandled state.  This is not
    /// expected to happen, and if it does, it indicates a code maintenance error.
    /// </para>
    /// </summary>
    /// <param name="filePath">
    /// File path of the Markdown file to load.
    /// </param>
    /// <param name="newlineStrategy">
    /// Option for how to manage newlines when modified Markdown files are written.
    /// </param>
    /// <returns>MdStruct</returns>
    /// <exception cref="NotImplementedException">
    /// </exception>
    public MdStruct Load(string filePath, NewlineStrategy newlineStrategy)
    {
        // Load the file content to parse.
        string fileContent = File.ReadAllText(filePath);

        // Based on file content and the user's preferred newline strategy, calculate the newline to use.
        // isModified is set to whether the file needs to be rewritten to reflect a change of newline sytle.
        string newline = Newline.DetermineNewline(newlineStrategy, fileContent, out bool isModified);

        // Reset the parser, making it ready for a run.
        Reset(NewlineRegion.Containing(newline));

        // Break the content into lines, for line-by-line processing.
        string[] lines = fileContent.Split(Constants.AllNewlines, StringSplitOptions.None);

        // Parse each line.  Note that each line DOES NOT have newline at end; these have been removed.
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            switch (_state)
            {
                case NormalState:
                    // This is a line of indented code.  It is important to check this first, or else
                    // an indented code block containing Markdown code would not be handled right.
                    if (line.StartsWith(Markers.FourSpaces) || line.StartsWith('\t'))
                    {
                        Save(new IndentedLineRegion(line));
                        continue;
                    }

                    if (trimmedLine.StartsWith(Markers.FenceMarker))
                    {
                        SaveToBuffer(line);
                        _state = InFencedCodeBlockState;
                        continue;
                    }

                    if (trimmedLine.StartsWith(Markers.BeginTocMarker))
                    {
                        SaveToBuffer(line);
                        _state = InTableOfContentsState;
                        continue;
                    }

                    // The line is "normal". Basically, its "normal" Markdown content that is
                    // not one of the other special things above (not indented code, not fenced,
                    // code, not TOC).  The LineParser is designed to handle this.
                    Save(_lineParser.Parse(line));
                    if (_lineParser.InHtmlComment)
                    {
                        _state = InHtmlCommentState;
                        continue;
                    }
                    break;

                case InTableOfContentsState:
                    SaveToBuffer(line);
                    if (trimmedLine.StartsWith(Markers.EndTocMarker))
                    {
                        TocRegion tocRegion = new(_regionContent.ToString());
                        Save(tocRegion);
                        _regionContent.Clear();
                        _state = NormalState;
                        continue;
                    }
                    break;

                case InFencedCodeBlockState:
                    SaveToBuffer(line);
                    if (trimmedLine.StartsWith(Markers.FenceMarker))
                    {
                        FencedRegion fencedRegion = new(_regionContent.ToString());
                        Save(fencedRegion);
                        _regionContent.Clear();
                        _state = NormalState;
                        continue;
                    }
                    break;

                case InHtmlCommentState:
                    Save(_lineParser.Parse(line));
                    if (!_lineParser.InHtmlComment)
                    {
                        _state = NormalState;
                        continue;
                    }
                    break;

                default:
                    throw new NotImplementedException($"Unhandled {nameof(_state)}: {_state}");
            }
        }

        // Assemble and return the MdStruct
        MdStruct mdStruct = new(filePath, _regions, isModified, _newlineRegion);
        return mdStruct;
    }

    private void SaveToBuffer(string line)
    {
        if (_regionContent.Length > 0)
            _regionContent.Append(_newlineRegion.Content);
        _regionContent.Append(line);
    }

    private void Save(List<Region> lineRegions)
    {
        Separate();
        foreach (Region region in lineRegions)
        {
            _regions.Add(region);
        }
    }

    private void Save(Region region)
    {
        Separate();
        _regions.Add(region);
    }

    private void Separate()
    {
        if (_regions.Count > 0)
        {
            _regions.Add(_newlineRegion);
        }
    }
}
