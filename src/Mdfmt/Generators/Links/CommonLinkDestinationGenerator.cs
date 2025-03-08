using System.Text.RegularExpressions;

namespace Mdfmt.Generators.Links;

internal class CommonLinkDestinationGenerator : ILinkDestinationGenerator
{
    private static readonly Regex RemoveCharactersRegex = new(@"[.\'(),:+?/]", RegexOptions.Compiled);

    public string SlugifyHeadingText(string headingText)
    {
        return RemoveCharactersRegex.Replace(headingText, string.Empty).Replace(" ", "-").ToLower();
    }

    public string GenerateInDocumentLinkDestination(string filename, string headingText)
    {
        return $"#{SlugifyHeadingText(headingText)}";
    }

    public string GenerateCrossDocumentLinkDestination(string relativeFilePath, string headingText)
    {
        return $"{relativeFilePath}#{SlugifyHeadingText(headingText)}";
    }
}
