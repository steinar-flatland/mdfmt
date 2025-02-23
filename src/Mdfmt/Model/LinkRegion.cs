using Mdfmt.Utilities;
using System;

namespace Mdfmt.Model;

internal class LinkRegion(string label, string destination) : MutableStringRegion(AssembleLink(label, destination))
{
    private static string AssembleLink(string label, string destination)
    {
        // Null destination would be really weird.  Don't allow this.
        ArgumentNullException.ThrowIfNull(destination);

        return $"[{label}]({destination})";
    }

    public override string Content
    {
        set
        {
            //NOTE:  If we ever do want to set the whole content of the link at once, then:
            // 1. SetContent(value);
            // 2. Parse this value and assign the pieces to Label and _destination below to 
            // maintain consistency.  Throw if not parseable.
            //
            // Since, at this time, we don't really care to set the whole content of a link at once,
            // throw here for now.

            throw new NotImplementedException($"Class {nameof(LinkRegion)} does not implement set" +
                $" {nameof(Content)} at this time.  It could be implemented if necessary.");
        }
    }

    public string Label { get; } = label;

    private string _destination = destination;

    public string Destination
    {
        get => _destination;
        set
        {
            _destination = value;
            string newLinkValue = AssembleLink(Label, _destination);
            SetContent(newLinkValue);
        }
    }

    /// <summary>
    /// If the <c>Destination</c> includes a <c>'#'</c> character, the 0 or more characters from front of 
    /// <c>Destination</c> up to but not including <c>'#'</c>.  If there is no <c>'#'</c>, then returns
    /// the entire <c>Destination</c>.
    /// </summary>
    public string Path
    {
        get
        {
            int index = _destination.IndexOf('#');
            return (index >= 0) ? _destination[..index] : _destination;
        }
    }

    /// <summary>
    /// If the <c>Destination</c> contains a <c>'#'</c> character, everything from and including
    /// the <c>'#'</c> to end of <c>Destination</c>.  If there is no <c>'#'</c> then empty string.
    /// </summary>
    public string Fragment
    {
        get
        {
            int index = _destination.IndexOf('#');
            return (index >= 0) ? _destination[index..] : "";
        }
    }

    /// <summary>
    /// Determine the type of this link, based on the state of this <c>LinkRegion</c> and the simple
    /// file name in which the link occurs.
    /// </summary>
    /// <param name="fileName">
    /// The simple file name, without any path, in which this link occurs.  e.g. <c>"Readme.md"</c>.
    /// </param>
    /// <returns>
    /// <see cref="LinkType"/> enum value, indicating <c>InDocument</c>, <c>CrossDocument</c>, or <c>External</c>.
    /// </returns>
    public LinkType GetLinkType(string fileName)
    {
        string path = Path;
        if ((path.Length == 0) || (path == $"./{fileName}") || (path == fileName))
        {
            return LinkType.InDocument;
        }
        else if (path.EndsWith(Constants.MdExtension))
        {
            return LinkType.CrossDocument;
        }
        else
        {
            return LinkType.External;
        }
    }

    /// <summary>
    /// <para>
    /// Given the <see cref="MdStruct"/> in which this <see cref="LinkRegion"/> occurs, determine
    /// the cpath of the link destination.
    /// </para>
    /// <para>
    /// (An <see cref="InvalidOperationException"/> occurs if there is an unhandled <see cref="LinkType"/>.
    /// This should not happen and would indicate a code maintenance error.)
    /// </para>
    /// </summary>
    /// <param name="md">
    /// The <see cref="MdStruct"/> in which this link region occurs.
    /// </param>
    /// <returns>
    /// Canonical relative path of the link target file, or null if the link is external.
    /// </returns>
    /// <exception cref="InvalidOperationException"/>
    public string DestinationCpath(MdStruct md)
    {
        // Path portion of the link destination.
        string path = Path;

        // Goal is to determine this and return it.
        string targetCpath;

        // Classify link as Indocument, CrossDocument, or External.
        LinkType linkType = GetLinkType(md.FileName);

        switch (linkType)
        {
            case LinkType.InDocument:
                targetCpath = md.Cpath;
                break;
            case LinkType.CrossDocument:
                if (path.StartsWith('.'))
                {
                    targetCpath = PathUtils.Canonicalize(md.Cpath, path);
                }
                else
                {
                    targetCpath = PathUtils.Canonicalize(md.Cpath, $"./{path}");
                }
                break;
            case LinkType.External:
                targetCpath = null;
                break;
            default:
                throw new InvalidOperationException($"Unknown LinkType: {linkType}");
        }

        return targetCpath;
    }

}
