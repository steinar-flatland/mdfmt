using System;
using System.Text;

namespace Mdfmt.Loaders;

/// <summary>
/// <para>
/// This parser is used to break down the content of a text file into a sequence of strings.
/// Tokens returned comprise:  (1) Each non-empty line without trailing newline, and
/// (2) each newline character or character sequence.
/// </para>
/// <para>
/// Lifecycle: Use this parser to parse one document only.  Construct it with the content to parse.
/// Repeatedly call Parse() to get each successive token.  When Parse() returns null, there is no
/// more data, and the <c>FileContentParser</c> should be discarded.
/// </para>
/// </summary>
internal class FileContentParser(string content)
{
    /// <summary>
    /// The content being parsed
    /// </summary>
    private readonly string _content = content;

    /// <summary>
    /// Index into _content where parsing.
    /// </summary>
    private int _offset;

    /// <summary>
    /// Token being built up during current call to Parse()
    /// </summary>
    private readonly StringBuilder _token = new();

    /// <summary>
    /// Parser state.
    /// </summary>
    private int _state;

    /// <summary>
    /// Prepare for parsing the next token.
    /// </summary>
    private void Reset()
    {
        _token.Clear();
        _state = 0;
    }

    /// <summary>
    /// <para>
    /// Parse the next string from file content, which is either non-empty line content without
    /// a trailing newline, or it is a single newline character or character sequence.  Call
    /// this method repeatedly on the content of a text file, until null is returned to signal
    /// end of data.
    /// </para>
    /// <para>
    /// Throws <c>NotImplementedException</c> if parser enters an unhandled state.  This is not
    /// expected to happen, and if it does, it indicates a code maintenance error.
    /// </para>
    /// </summary>
    /// <returns>
    /// Next string parsed or null to signal end of data.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// </exception>
    public string Parse()
    {
        Reset();

        char ch;

        while (_offset < _content.Length)
        {
            ch = _content[_offset++];
            _token.Append(ch);

            switch (_state)
            {
                case 0:
                    switch (ch)
                    {
                        case '\r':
                            _state = 1;
                            break;
                        case '\n':
                            return _token.ToString();
                        default:
                            _state = 2;
                            break;
                    }
                    break;
                case 1:
                    if (ch == '\n')
                        return _token.ToString();
                    else
                        _state = 2;
                    break;
                case 2:
                    if (ch == '\r' || ch == '\n')
                    {
                        // put 1 character back
                        _offset--;
                        _token.Length = _token.Length - 1;
                        return _token.ToString();
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unhandled parser state: {_state}");
            }
        }

        return _token.Length > 0 ? _token.ToString() : null;
    }
}
