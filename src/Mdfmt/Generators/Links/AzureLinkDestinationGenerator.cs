using System.Text.RegularExpressions;

namespace Mdfmt.Generators.Links;

internal class AzureLinkDestinationGenerator : ILinkDestinationGenerator
{
    private static readonly Regex SimpleDestinationRegex = new(@"^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    public string SlugifyHeadingText(string headingText)
    {
        return headingText.Replace(" ", "-").ToLower();
    }

    public string GenerateInDocumentLinkDestination(string filename, string headingText)
    {

        string slug = SlugifyHeadingText(headingText);

        string destination;
        if (SimpleDestinationRegex.IsMatch(slug))
        {
            destination = $"#{slug}";
        }
        else
        {
            destination = $"./{filename}#{slug}";
        }
        return destination;
    }

    public string GenerateCrossDocumentLinkDestination(string relativeFilePath, string headingText)
    {
        return $"{relativeFilePath}#{SlugifyHeadingText(headingText)}";
    }
}
