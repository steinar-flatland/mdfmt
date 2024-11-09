using System.Text.RegularExpressions;

namespace Mdfmt.Generators.Links;

internal class AzureLinkDestinationGenerator : ILinkDestinationGenerator
{
    private static readonly Regex SimpleDestinationRegex = new(@"^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    public string GenerateLinkDestination(string filename, string headingText)
    {
        string destination = headingText.Replace(" ", "-").ToLower();

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
}
