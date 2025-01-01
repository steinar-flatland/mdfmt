using Mdfmt.Model;
using Mdfmt.Options;
using Mdfmt.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mdfmt.Loaders;

/// <summary>
/// Loads a single Markdown file into a MdStruct data structure.  This class is implemented in a
/// reusable way, i.e., it is safe to call the <c>Load()</c> method, to load a Markdown file,
/// multiple times on one instance of this class.
/// </summary>
internal class MdStructLoader
{
    #region Parser_States

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

    #endregion Parser_States

    #region Resettable_State

    // This state is reset on each call to the Load() method.

    /// <summary>
    /// Breaks down file content into tokens comprising (1) each non-empty line without a trailing 
    /// newline, and (2) each newline character or character sequence.
    /// </summary>
    private FileContentParser _fileContentParser;

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
    /// Reset the loader at start of load of a Markdown file.  This makes it safe to call the
    /// Load() method repeatedly on the same instance of this class.
    /// </summary>
    /// <param name="fileContent"">
    /// Content of the file being parsed.
    /// </param>
    /// <param name="newlineRegion">
    /// Newline region to use for line breaks.
    /// </param>
    private void Reset(string fileContent, NewlineRegion newlineRegion)
    {
        _fileContentParser = new FileContentParser(fileContent);
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
        Reset(fileContent, NewlineRegion.Containing(newline));

        // The token most recently parsed by the _fileContentParser.  This is either the text of a
        // non-empty line (without trailing newline), or it is a newline character or sequence.
        string token;

        // Whether the token is a newline.
        bool isNewline;

        // If the token is a non-empty line (not a newline) the text of the line, else null.
        string line;

        // If line is not null, the trimmed version of the line.
        string trimmedLine;

        while ((token = _fileContentParser.Parse()) != null)
        {
            isNewline = Constants.AllNewlines.Contains(token);
            line = isNewline ? null : token;
            trimmedLine = isNewline ? null : token.Trim();

            switch (_state)
            {
                case NormalState:
                    if (isNewline)
                    {
                        _regions.Add(_newlineRegion);
                        continue;
                    }

                    // This is a line of indented code.  It is important to check this first, or else
                    // an indented code block containing Markdown code would not be handled right.
                    if (line.StartsWith(Markers.FourSpaces) || line.StartsWith('\t'))
                    {
                        _regions.Add(new IndentedLineRegion(line));
                        continue;
                    }

                    if (trimmedLine.StartsWith(Markers.FenceMarker))
                    {
                        _regionContent.Append(line);
                        _state = InFencedCodeBlockState;
                        continue;
                    }

                    if (trimmedLine.StartsWith(Markers.BeginTocMarker))
                    {
                        _regionContent.Append(line);
                        _state = InTableOfContentsState;
                        continue;
                    }

                    // The line is "normal". Basically, its "normal" Markdown content that is
                    // not one of the other special things above (not indented code, not fenced,
                    // code, not TOC).  The LineParser is designed to handle this.
                    RegionsAdd(_lineParser.Parse(line));
                    if (_lineParser.InHtmlComment)
                    {
                        _state = InHtmlCommentState;
                        continue;
                    }
                    break;

                case InTableOfContentsState:
                    if (isNewline)
                    {
                        _regionContent.Append(_newlineRegion.Content);
                        continue;
                    }
                    _regionContent.Append(line);
                    if (trimmedLine.StartsWith(Markers.EndTocMarker))
                    {
                        SaveTocRegion();
                        _state = NormalState;
                        continue;
                    }
                    break;

                case InFencedCodeBlockState:
                    if (isNewline)
                    {
                        _regionContent.Append(_newlineRegion.Content);
                        continue;
                    }
                    _regionContent.Append(line);
                    if (trimmedLine.StartsWith(Markers.FenceMarker))
                    {
                        SaveFencedRegion();
                        _state = NormalState;
                        continue;
                    }
                    break;

                case InHtmlCommentState:
                    if (isNewline)
                    {
                        _regions.Add(_newlineRegion);
                        continue;
                    }
                    RegionsAdd(_lineParser.Parse(line));
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

        MopUp();

        // Assemble and return the MdStruct
        MdStruct mdStruct = new(filePath, _regions, isModified, _newlineRegion);
        return mdStruct;
    }

    private void RegionsAdd(List<Region> regions)
    {
        foreach (Region region in regions)
        {
            _regions.Add(region);
        }
    }

    private void SaveFencedRegion()
    {
        FencedRegion fencedRegion = new(_regionContent.ToString());
        _regions.Add(fencedRegion);
        _regionContent.Clear();
    }

    private void SaveTocRegion()
    {
        string regionContent = _regionContent.ToString();
        TocRegion tocRegion = new(regionContent);
        _regions.Add(tocRegion);
        _regionContent.Clear();
    }

    private void MopUp()
    {
        // Avoid losing left over state in _regionContent.  This is an issue if the Markdwon file
        // has either an unclosed fenced code block or a table of contents region that has an
        // opening comment but no closing comment.
        if (_regionContent.Length > 0)
        {
            if (_state == InFencedCodeBlockState)
            {
                SaveFencedRegion();
            }
            else if (_state == InTableOfContentsState)
            {
                SaveTocRegion();
            }
        }
    }

}
