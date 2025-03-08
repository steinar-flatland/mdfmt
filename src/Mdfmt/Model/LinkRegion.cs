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
    /// Whether the destination starts with http (case-insensitive).
    /// </summary>
    private bool DestinationIsUrl => _destination.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// <para>
    /// Returns local file system path of link destination.
    /// </para>
    /// <para>
    /// If the <c>Destination</c> looks like a URL, returns empty string.
    /// </para>
    /// <para>
    /// Otherwise, it is assumed that the <c>Destination</c> is trying to target a file in the local
    /// file system.  Then, if the <c>Destination</c> includes a <c>'#'</c> character, return the 0
    /// or more characters from front of <c>Destination</c> up to but not including <c>'#'</c>, and
    /// if there is no <c>'#'</c>, then returns the entire <c>Destination</c>.
    /// </para>
    /// </summary>
    public string Path
    {
        get
        {
            if (DestinationIsUrl)
            {
                return string.Empty;
            }
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
            return (index >= 0) ? _destination[index..] : string.Empty;
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
        if (string.IsNullOrWhiteSpace(_destination))
        {
            return LinkType.InDocument;
        }

        if (_destination.StartsWith('/'))
        {
            // Mdfmt chooses not to deal with absolute paths, since many Markdown rendering
            // environments don't really deal with them.
            return LinkType.External;
        }

        if (DestinationIsUrl)
        {
            return LinkType.External;
        }

        // The path seems to be referencing something in the local file system.

        string path = Path;

        if ((path.Length == 0) || (path == $"./{fileName}") || (path == fileName))
        {
            return LinkType.InDocument;
        }

        if (path.EndsWith(Constants.MdExtension))
        {
            return LinkType.CrossDocument;
        }

        // Things that remain seem to be links within the file system, but not to Markdown files.
        // Lump these in with External, i.e. they (like http) are external to the local web of 
        // Markdown files that occur under the processing root.
        return LinkType.External;
    }

    /// <summary>
    /// <para>
    /// Given the <see cref="MdStruct"/> in which this <see cref="LinkRegion"/> occurs, determine
    /// the cpath of the link destination.  A cpath is a canonical relative path, relative to the
    /// processing root that defines the files in the file system that are visible to the program.
    /// </para>
    /// <para>
    /// An <see cref="InvalidOperationException"/> occurs if there is an unhandled <see cref="LinkType"/>.
    /// This should not happen and would indicate a code maintenance error.
    /// </para>
    /// </summary>
    /// <param name="md">
    /// The <see cref="MdStruct"/> in which this link region occurs.
    /// </param>
    /// <returns>
    /// <list type="bullet">
    /// <item>
    /// If in-document link: cpath of the document that contains the link.
    /// </item>
    /// <item>
    /// If cross-document link:  cpath of the document that that the link is targeting.
    /// </item>
    /// <item>
    /// If would-be cross-document link, but we are unable to canonicalize it: A string containing
    /// an exclamation point <c>"!"</c>.
    /// </item>
    /// <item>
    /// If external link: <c>null</c>
    /// </item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException"/>
    public string DestinationCpath(MdStruct md)
    {
        // Classify link as InDocument, CrossDocument, or External.
        LinkType linkType = GetLinkType(md.FileName);

        // Goal is to determine this and return it.
        string destinationCpath = linkType switch
        {
            LinkType.InDocument => md.Cpath,
            LinkType.CrossDocument => PathUtils.Canonicalize(md.Cpath, Path) ?? "!",
            LinkType.External => null,
            _ => throw new InvalidOperationException($"Unknown LinkType: {linkType}"),
        };

        return destinationCpath;
    }

}
