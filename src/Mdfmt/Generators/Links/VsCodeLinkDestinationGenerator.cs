using System.Text.RegularExpressions;

namespace Mdfmt.Generators.Links;

internal partial class VsCodeLinkDestinationGenerator : ILinkDestinationGenerator
{
    private static readonly Regex RemoveCharactersRegex = new(@"[.\'(),:+?]", RegexOptions.Compiled);

    public string GenerateLinkDestination(string filename, string headingText)
    {
        string destination = '#' + headingText;
        destination = RemoveCharactersRegex.Replace(destination, "").Replace(" ", "-").ToLower();
        return destination;
    }
}
