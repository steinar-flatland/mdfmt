using Mdfmt.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mdfmt.Generators.Links;

public static class LinkDestinationGeneratorFactory
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

    /// <summary>
    /// Make and return one of each concrete class that implements <c>ILinkDestinationGenerator</c>.
    /// </summary>
    /// <returns>List of <c>ILinkDestinationGenerator</c></returns>
    public static List<ILinkDestinationGenerator> ManufactureOneOfEach()
    {
        Type interfaceType = typeof(ILinkDestinationGenerator);
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        IEnumerable<Type> types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => interfaceType.IsAssignableFrom(type)
                           && type.IsClass
                           && !type.IsAbstract
                           && type.GetConstructor(Type.EmptyTypes) != null);
        List<ILinkDestinationGenerator> result = [];
        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is ILinkDestinationGenerator instance)
            {
                result.Add(instance);
            }
        }
        return result;
    }
}
