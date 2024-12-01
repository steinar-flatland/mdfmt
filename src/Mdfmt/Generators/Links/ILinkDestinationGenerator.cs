namespace Mdfmt.Generators.Links;

/// <summary>
/// Interface for generating Markdown link destinations.  A Markdown link has two parts:
/// [label](destination).  This generator generates the destination part for inside the parentheses.
/// </summary>
internal interface ILinkDestinationGenerator
{
    /// <summary>
    /// Generate a destination for a Markdown link.
    /// </summary>
    /// <param name="filename">The simple name of the file being processed without path, e.g. "Readme.md"</param>
    /// <param name="headingText">The text of the Markdown heading, nice and clean, the way it displays to the user.</param>
    /// <returns>Link destination for that heading</returns>
    public string GenerateLinkDestination(string filename, string headingText);
}
