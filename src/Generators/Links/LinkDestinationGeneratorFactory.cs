using Mdfmt.Options;
using System;

namespace Mdfmt.Generators.Links;

internal static class LinkDestinationGeneratorFactory
{
    public static ILinkDestinationGenerator Manufacture(Platform platform)
    {
        return platform switch
        {
            Platform.VsCode => new VsCodeLinkDestinationGenerator(),
            Platform.Azure => new AzureLinkDestinationGenerator(),
            _ => throw new InvalidOperationException($"Unsupported {nameof(Platform)} value: {platform}"),
        };
    }
}
