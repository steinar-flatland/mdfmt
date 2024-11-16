using Mdfmt.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mdfmt.Loaders;

/// <summary>
/// <para>
/// Parser for a line of Markdown while parsing a single Markdown document.
/// </para>
/// <para>
/// Lifecyle: An instance of this class is intended to parse multiple lines of Markdown from the
/// same Markdown document.  It is safe to call <c>Parse(string line)</c> repeatedly on the same
/// instance of <c>LineParser</c>, as long as the lines are from the same Markdown document.  Once
/// the end of Markdown document is reached, this <c>LineParser</c> should be discarded.
/// </para>
/// <para>
/// Apply this parser to lines that are NOT indented code, fenced code, or a TOC.  
/// </para>
/// <para>
/// Limitation: This parser does not deal with double backticks yet.
/// </para>
/// </summary>
public class LineParser
{
    /// <summary>
    /// Buffer for saving parsed content that is not from links.
    /// </summary>
    private readonly StringBuilder _buffer = new();

    /// <summary>
    /// Buffer for saving parsed content that looks like it is from links.
    /// </summary>
    private readonly StringBuilder _linkBuffer = new();

    /// <summary>
    /// Builds up content of link label
    /// </summary>
    private readonly StringBuilder _labelBuffer = new();

    /// <summary>
    /// Builds up content of link destination
    /// </summary>
    private readonly StringBuilder _destinationBuffer = new();

    private const int NotInCommentState = 1;
    private const int StartingCommentState1 = 2;
    private const int StartingCommentState2 = 3;
    private const int StartingCommentState3 = 4;
    private const int InCommentState = 5;
    private const int EndingCommentState1 = 6;
    private const int EndingCommentState2 = 7;
    private const int InSingleBacktickState = 10;
    private const int StartingImageLinkState = 11;
    private const int LabelState = 12;
    private const int LabelDestinationTransitionState = 13;
    private const int DestinationState = 14;

    /// <summary>
    /// State of the parser
    /// </summary>
    private int _state;

    /// <summary>
    /// Whether in an HTML comment.  Initially this is false, and it is updated after each call to 
    /// <c>Parse(string line)</c>, to indicate whether the line ended inside a commented region or
    /// not.  This knowledge is important for starting the parser in the right state.
    /// </summary>
    public bool InHtmlComment { get; private set; }

    /// <summary>
    /// Reset the state of the parser to prepare for parsing the next line.
    /// </summary>
    private void Reset()
    {
        _buffer.Clear();
        _linkBuffer.Clear();
        _labelBuffer.Clear();
        _destinationBuffer.Clear();
        _state = InHtmlComment ? InCommentState : NotInCommentState;
    }

    /// <summary>
    /// <para>
    /// Parse a line of Markdown into a list of 0 or more regions.  If the line is empty, an empty
    /// list is returned.
    /// </para>
    /// <para>
    /// Throws <c>NotImplementedException</c> if the parser enters an unhandled state.  This is not
    /// expected to happen, and if it does, it indicates a code maintenance error.
    /// </para>
    /// </summary>
    /// <param name="line">The line of Markdown to parse.</param>
    /// <returns>A list of 0 or more <c>Region</c>.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<Region> Parse(string line)
    {
        Reset();

        List<AtomicRegion> atomicRegions = [];

        foreach (char ch in line)
        {
            switch (_state)
            {
                case NotInCommentState:
                    switch (ch)
                    {
                        case '<':
                            _buffer.Append(ch);
                            _state = StartingCommentState1;
                            break;
                        case '!':
                            _linkBuffer.Append(ch);
                            _state = StartingImageLinkState;
                            break;
                        case '[':
                            _linkBuffer.Append(ch);
                            _state = LabelState;
                            break;
                        case '`':
                            _buffer.Append(ch);
                            _state = InSingleBacktickState;
                            break;
                        default:
                            _buffer.Append(ch);
                            break;
                    }
                    break;
                case StartingCommentState1:
                    _buffer.Append(ch);
                    _state = (ch == '!') ? StartingCommentState2 : NotInCommentState;
                    break;
                case StartingCommentState2:
                    _buffer.Append(ch);
                    _state = (ch == '-') ? StartingCommentState3 : NotInCommentState;
                    break;
                case StartingCommentState3:
                    _buffer.Append(ch);
                    if (ch == '-')
                    {
                        if (_buffer.Length > 4)
                        {
                            // _buffer has characters before the open HTML comment sequence <!--
                            // that need to be turned into a ContentRegion.
                            atomicRegions.Add(new ContentRegion(_buffer.ToString(0, _buffer.Length - 4)));
                            _buffer.Remove(0, _buffer.Length - 4);
                        }
                        _state = InCommentState;
                    }
                    else
                    {
                        _state = NotInCommentState;
                    }
                    break;
                case InCommentState:
                    _buffer.Append(ch);
                    if (ch == '-')
                    {
                        _state = EndingCommentState1;
                    }
                    break;
                case EndingCommentState1:
                    _buffer.Append(ch);
                    _state = (ch == '-') ? EndingCommentState2 : InCommentState;
                    break;
                case EndingCommentState2:
                    _buffer.Append(ch);
                    if (ch == '>')
                    {
                        // _buffer has HTML comment content that needs to be saved as a CommentRegion.
                        atomicRegions.Add(new CommentRegion(_buffer.ToString()));
                        _buffer.Clear();
                        _state = NotInCommentState;
                    }
                    else
                    {
                        _state = InCommentState;
                    }
                    break;
                case InSingleBacktickState:
                    _buffer.Append(ch);
                    if (ch == '`')
                    {
                        _state = NotInCommentState;
                    }
                    break;
                case StartingImageLinkState:
                    _linkBuffer.Append(ch);
                    if (ch == '[')
                    {
                        _state = LabelState;
                    }
                    else
                    {
                        _buffer.Append(_linkBuffer);
                        _linkBuffer.Clear();
                        _state = NotInCommentState;
                    }
                    break;
                case LabelState:
                    _linkBuffer.Append(ch);
                    if (ch == ']')
                    {
                        _state = LabelDestinationTransitionState;
                    }
                    else
                    {
                        _labelBuffer.Append(ch);
                    }
                    break;
                case LabelDestinationTransitionState:
                    _linkBuffer.Append(ch);
                    if (ch == '(')
                    {
                        _state = DestinationState;
                    }
                    else
                    {
                        _buffer.Append(_linkBuffer);
                        _linkBuffer.Clear();
                        _labelBuffer.Clear();
                        _state = NotInCommentState;
                    }
                    break;
                case DestinationState:
                    _linkBuffer.Append(ch);
                    if (ch == ')')
                    {
                        // There is a link in the link buffer.
                        string link = _linkBuffer.ToString();
                        string label = _labelBuffer.ToString();
                        string destination = _destinationBuffer.ToString();
                        if (link.StartsWith('!'))
                        {
                            // Link to image.  We don't do anything with these.  
                            // Treat this as regular content.
                            _buffer.Append(_linkBuffer);
                        }
                        else
                        {
                            // If there was stuff in the other buffer, need to save that as a ContentRegion.
                            if (_buffer.Length > 0)
                            {
                                atomicRegions.Add(new ContentRegion(_buffer.ToString()));
                                _buffer.Clear();
                            }
                            // Link to heading.  Save a LinkRegion.
                            atomicRegions.Add(new LinkRegion(label, destination));
                        }
                        _linkBuffer.Clear();
                        _labelBuffer.Clear();
                        _destinationBuffer.Clear();
                        _state = NotInCommentState;
                    }
                    else
                    {
                        _destinationBuffer.Append(ch);
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unhandled parser state: {_state}");
            }
        } // end foreach (char ch in line)

        // Handle different requirements for what should happen at end of line based on parser state
        switch (_state)
        {
            case NotInCommentState:
            case StartingCommentState1:
            case StartingCommentState2:
            case StartingCommentState3:
            case InSingleBacktickState:
                // Be careful not to generate an empty region.
                if (_buffer.Length > 0)
                {
                    atomicRegions.Add(new ContentRegion(_buffer.ToString()));
                }
                InHtmlComment = false;
                break;
            case StartingImageLinkState:
            case LabelState:
            case LabelDestinationTransitionState:
            case DestinationState:
                // Truncated link is just normal content.
                _buffer.Append(_linkBuffer);
                atomicRegions.Add(new ContentRegion(_buffer.ToString()));
                InHtmlComment = false;
                break;
            case InCommentState:
            case EndingCommentState1:
            case EndingCommentState2:
                // Unclosed comment at end of line just becomes a comment region.
                // Next line will pick up parsing in state InCommentState.
                // Be careful not to generate an empty region.
                if (_buffer.Length > 0)
                {
                    atomicRegions.Add(new CommentRegion(_buffer.ToString()));
                }
                InHtmlComment = true;
                break;
            default:
                throw new InvalidOperationException($"Unhandled parser state: {_state}");
        }

        List<Region> regions = atomicRegions.IsHeading() ?
            [new HeadingRegion(atomicRegions)] :
            atomicRegions.Cast<Region>().ToList();
        return regions;
    }

    /// <summary>
    /// <para>
    /// Parse a line of Markdown into a list of 0 or more regions.  If the line is empty, an empty
    /// list is returned.  Use this variant only if you need to control the initial state of 
    /// <c>InHtmlComment</c>.  This is provided mainly for testability.
    /// </para>
    /// <para>
    /// Throws <c>NotImplementedException</c> if the parser enters an unhandled state.  This is not
    /// expected to happen, and if it does, it indicates a code maintenance error.
    /// </para>
    /// </summary>
    /// <param name="line">The line of Markdown to parse.</param>
    /// <param name="inHtmlComment">Whether in HTML comment at start of parse.</param>
    /// <returns>A list of 0 or more <c>Region</c>.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<Region> Parse(string line, bool inHtmlComment)
    {
        InHtmlComment = inHtmlComment;
        return Parse(line);
    }
}
