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
    /// The newline strategy to use for loading this Markdown file, or null if no preference.
    /// </summary>
    private NewlineStrategy? _newlineStrategy;

    /// <summary>
    /// <see cref="NewlineRegion"/> to forward to <see cref="MdStruct"/> constructor, to know what
    /// newline to use when making any changes to the loaded Markdown file.  Only in the case when
    /// <c>_newlineStrategy</c> is non-null, <c>_newlineRegion</c> reflects the target newline 
    /// character of the strategy considered in light of the file being loaded, and in this case
    /// any newlines loaded from file will be replaced by this <c>_newlineRegion</c> when loading.
    /// </summary>
    private NewlineRegion _newlineRegion;

    /// <summary>
    /// Parser for a line of Markdown that is not code or a TOC.
    /// </summary>
    private LineParser _lineParser;

    /// <summary>
    /// A sequence of content regions parsed from the Markdown file.
    /// </summary>
    private readonly List<Region> _regions = [];

    /// <summary>
    /// State that controls the behavior of the parser.
    /// </summary>
    private int _state;

    /// <summary>
    /// Builds up the content of a table of contents region, which spans multiple lines.
    /// </summary>
    private readonly StringBuilder _regionContent = new();

    /// <summary>
    /// Builds up the state of a fenced region, which spans multiple lines.
    /// </summary>
    private readonly FencedRegionBuilder _fencedRegionBuilder = new();

    /// <summary>
    /// Reset the loader at start of load of a Markdown file.  This makes it safe to call the
    /// Load() method repeatedly on the same instance of this class.
    /// </summary>
    /// <param name="fileContent"">
    /// Content of the file being parsed.
    /// </param>
    /// <param name="newlineStrategy">
    /// The newline strategy to use for loading this Markdown file, or null if no preference.
    /// </param>
    /// <param name="newlineRegion">
    /// See discussion of <c>_newlineRegion</c> above.
    /// </param>
    private void Reset(string fileContent, NewlineStrategy? newlineStrategy, NewlineRegion newlineRegion)
    {
        _fileContentParser = new FileContentParser(fileContent);
        _newlineStrategy = newlineStrategy;
        _newlineRegion = newlineRegion;
        _lineParser = new();
        _regions.Clear();
        _state = NormalState;
        _regionContent.Clear();
        _fencedRegionBuilder.Clear();
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
    /// The newline strategy to use for loading this Markdown file, or null if no preference.
    /// </param>
    /// <returns>MdStruct</returns>
    /// <exception cref="NotImplementedException">
    /// </exception>
    public MdStruct Load(string filePath, NewlineStrategy? newlineStrategy)
    {
        // Load the file content to parse.
        string fileContent = File.ReadAllText(filePath);

        // Based on file content and the user's preferred newline strategy, calculate the newline to use.
        // isModified is set to whether the file needs to be rewritten to reflect a change of newline sytle.
        string newline = Newline.DetermineNewline(newlineStrategy, fileContent, out bool isModified);

        // Reset the parser, making it ready for a run.
        Reset(fileContent, newlineStrategy, NewlineRegion.Containing(newline));

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
                        _regions.Add(GetNewlineRegion(token));
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
                        _fencedRegionBuilder.AddRegion(new FenceMarkerRegion(line));
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
                    }
                    break;

                case InTableOfContentsState:
                    if (isNewline)
                    {
                        _regionContent.Append(GetNewlineRegion(token).Content);
                        continue;
                    }
                    _regionContent.Append(line);
                    if (trimmedLine.StartsWith(Markers.EndTocMarker))
                    {
                        SaveTocRegion();
                        _state = NormalState;
                    }
                    break;

                case InFencedCodeBlockState:
                    if (isNewline)
                    {
                        _fencedRegionBuilder.AddRegion(GetNewlineRegion(token));
                        continue;
                    }
                    if (trimmedLine.StartsWith(Markers.FenceMarker))
                    {
                        _fencedRegionBuilder.AddRegion(new FenceMarkerRegion(line));
                        SaveFencedRegion();
                        _state = NormalState;
                    }
                    else
                    {
                        _fencedRegionBuilder.AddRegion(new ContentRegion(line));
                    }
                    break;

                case InHtmlCommentState:
                    if (isNewline)
                    {
                        _regions.Add(GetNewlineRegion(token));
                        continue;
                    }
                    RegionsAdd(_lineParser.Parse(line));
                    if (!_lineParser.InHtmlComment)
                    {
                        _state = NormalState;
                    }
                    break;

                default:
                    throw new NotImplementedException($"Unhandled {nameof(_state)}: {_state}");
            }
        }

        MopUp();

        // Assemble and return the MdStruct
        List<Region> regionsShallowCopy = new(_regions);
        MdStruct mdStruct = new(filePath, regionsShallowCopy, isModified, _newlineRegion);
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
        FencedRegion fencedRegion = _fencedRegionBuilder.BuildFencedRegion();
        _regions.Add(fencedRegion);
        // BuildFencedRegion() already cleared the _fencedRegionBuilder
    }

    private void SaveTocRegion()
    {
        TocRegion tocRegion = new(_regionContent.ToString());
        _regions.Add(tocRegion);
        _regionContent.Clear();
    }

    private void MopUp()
    {
        // Avoid losing left over state in _regionContent.  This is an issue if the Markdown file
        // has either an unclosed fenced code block or a table of contents region that has an
        // opening comment but no closing comment.
        if (_fencedRegionBuilder.HasRegions)
        {
            SaveFencedRegion();
        }
        else if (_regionContent.Length > 0)
        {
            SaveTocRegion();
        }
    }

    /// <summary>
    /// Determine the Newline region to use, given a current token that is a newline character or
    /// sequence parsed from the Markdown file.
    /// </summary>
    /// <param name="token">string containing newline character or sequence just parsed</param>
    /// <returns>Region containing newline chracter or sequence to use.</returns>
    private NewlineRegion GetNewlineRegion(string token)
    {
        if (_newlineStrategy == null)
        {
            // Use the newline that was just parsed, since there is no other strategy in play.
            return NewlineRegion.Containing(token);
        }
        else
        {
            // Use the newline of this region that was already determined based on the
            // --newline-strategy option  and the content of Markdown file being processed.
            return _newlineRegion;
        }
    }

}
