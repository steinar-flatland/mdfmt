using System;

namespace Mdfmt.Model;

internal class LinkRegion(string label, string destination) : MutableStringRegion(AssembleLink(label, destination))
{
    private static string AssembleLink(string label, string destination)
    {
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
}
