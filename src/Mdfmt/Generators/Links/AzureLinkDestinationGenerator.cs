using System.Text.RegularExpressions;

namespace Mdfmt.Generators.Links;

internal class AzureLinkDestinationGenerator : ILinkDestinationGenerator
{
    private static readonly Regex SimpleDestinationRegex = new(@"^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    private static string Slugify(string headingText) => headingText.Replace(" ", "-").ToLower();

    public string GenerateInDocumentLinkDestination(string filename, string headingText)
    {
        string destination = Slugify(headingText);

        if (SimpleDestinationRegex.IsMatch(destination))
        {
            destination = $"#{destination}";
        }
        else
        {
            destination = $"./{filename}#{destination}";
        }
        return destination;
    }

    public string GenerateCrossDocumentLinkDestination(string relativeFilePath, string headingText)
    {
        return $"{relativeFilePath}#{Slugify(headingText)}";
    }
}
