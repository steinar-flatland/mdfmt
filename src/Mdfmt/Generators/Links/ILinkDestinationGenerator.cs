namespace Mdfmt.Generators.Links;

/// <summary>
/// Interface for generating Markdown link destinations.  A Markdown link has two parts:
/// [label](destination).  This generator generates the destination part for inside the parentheses.
/// </summary>
internal interface ILinkDestinationGenerator
{
    /// <summary>
    /// Generate a destination for a Markdown link to a heading within the same Markdown file.
    /// </summary>
    /// <param name="filename">
    /// The simple name of the file being processed without path, e.g. "README.md".
    /// </param>
    /// <param name="headingText">
    /// The text of the Markdown heading the way it displays to the user.
    /// </param>
    /// <returns>
    /// Link destination targeting the heading.
    /// </returns>
    public string GenerateInDocumentLinkDestination(string filename, string headingText);

    /// <summary>
    /// Generate a destination for a Markdown link to a heading within a different Markdown file.
    /// </summary>
    /// <param name="relativeFilePath">
    /// Relative path that starts with either <c>./</c> or <c>../</c>, continues with any other path
    /// segments delimited by forward slash, and ending with a Markdown (<c>.md</c>) file name.
    /// </param>
    /// <param name="headingText">
    /// The text of the Markdown heading the way it displays to the user.
    /// </param>
    /// <returns>
    /// Link destination targeting the heading.
    /// </returns>
    public string GenerateCrossDocumentLinkDestination(string relativeFilePath, string headingText);
}
