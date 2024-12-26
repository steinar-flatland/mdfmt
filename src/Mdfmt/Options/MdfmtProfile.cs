using Mdfmt.Utilities;
using System.Collections.Generic;
using System.Text.Json;

namespace Mdfmt.Options;

/// <summary>
/// Data loaded from .mdfmt file.
/// </summary>
internal class MdfmtProfile
    (
        Dictionary<string, FileProcessingOptions> options,
        Dictionary<string, string> cpathToOptions
    )
{
    /// <summary>
    /// Dictionary keyed on a name that maps to associated <see cref="FileProcessingOptions"/>.
    /// </summary>
    public Dictionary<string, FileProcessingOptions> Options { get; } = options ?? [];

    /// <summary>
    /// Dictionary mapping a canonical relative path (cpath) to a key to the <c>Options</c> dictionary,
    /// indicating the <see cref="FileProcessingOptions"/> that apply to the cpath and its substructure.
    /// </summary>
    public Dictionary<string, string> CpathToOptions { get; } = cpathToOptions ?? [];

    /// <summary>
    /// Try to find <see cref="FileProcessingOptions"/> for a cpath.
    /// </summary>
    /// <param name="cpath">
    ///   Canonical relative path of a Markdown file.
    /// </param>
    /// <param name="options">
    ///   Output parameter that is set to the <see cref="FileProcessingOptions"/> that were found or to
    ///   <c>null</c> if none found.
    /// </param>
    /// <returns>
    ///   <c>true</c> if and only if an object is being returned through the <c>options</c> output
    ///   parameter.  Note that a <c>true</c> return value does not guarantee that the returned
    ///   object is <em>complete</em>.  See the <c>IsComplete()</c> method of class
    ///   <c>FileProcessingOptions</c>.
    /// </returns>
    public bool TryGetFileProcessingOptions(string cpath, out FileProcessingOptions options)
    {
        // Start building an object to return.  Its properties are all null initially.
        FileProcessingOptions optionsToReturn = new();

        // Populate the object incrementally starting with the most specific FileProcessingOptions
        // based on the full cpath, and then trying incrementally shorter paths.  This implements
        // an inheritance model.
        foreach (string leftCpath in PathUtils.LeftPaths(cpath))
        {
            if (CpathToOptions.TryGetValue(leftCpath, out string optionsKey))
            {
                optionsToReturn.PopulateFrom(Options[optionsKey]);
                if (optionsToReturn.IsComplete())
                    break;
            }
        }

        bool populated = optionsToReturn.AnyPropertiesArePopulated();
        options = populated ? optionsToReturn : null;
        return populated;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, Constants.JsonSerializerOptions);
    }
}
