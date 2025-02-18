using System.Text.RegularExpressions;

namespace Mdfmt.Generators.Links;

internal class CommonLinkDestinationGenerator : ILinkDestinationGenerator
{
    private static readonly Regex RemoveCharactersRegex = new(@"[.\'(),:+?]", RegexOptions.Compiled);

    public string GenerateInDocumentLinkDestination(string filename, string headingText)
    {
        string destination = '#' + headingText;
        destination = RemoveCharactersRegex.Replace(destination, "").Replace(" ", "-").ToLower();
        return destination;
    }

    public string GenerateCrossDocumentLinkDestination(string relativeFilePath, string headingText)
    {
        return $"{relativeFilePath}{GenerateInDocumentLinkDestination(null, headingText)}";
    }
}
