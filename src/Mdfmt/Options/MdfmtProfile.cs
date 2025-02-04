using Mdfmt.Utilities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Mdfmt.Options;

/// <summary>
/// Data loaded from configuration file that optionally exists in the processing root directory.
/// </summary>
internal class MdfmtProfile
    (
        Dictionary<string, FormattingOptions> options,
        Dictionary<string, string> cpathToOptions
    )
{
    /// <summary>
    /// Dictionary keyed on a name that maps to associated <see cref="FormattingOptions"/>.
    /// </summary>
    public Dictionary<string, FormattingOptions> Options { get; } = new Dictionary<string, FormattingOptions>(options ?? [], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Dictionary mapping a canonical relative path (cpath) to a key to the <c>Options</c> dictionary,
    /// indicating the <see cref="FormattingOptions"/> that apply to the cpath and its substructure.
    /// </summary>
    public Dictionary<string, string> CpathToOptions { get; } = new Dictionary<string, string>(cpathToOptions ?? [], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Try to find <see cref="FormattingOptions"/> for a cpath.
    /// </summary>
    /// <param name="cpath">
    ///   Canonical relative path of a Markdown file.
    /// </param>
    /// <param name="options">
    ///   Output parameter that is set to the <see cref="FormattingOptions"/> that were found or to
    ///   <c>null</c> if none found.
    /// </param>
    /// <returns>
    ///   <c>true</c> if and only if an object is being returned through the <c>options</c> output
    ///   parameter.  Note that a <c>true</c> return value does not guarantee that the returned
    ///   object is <em>complete</em>.  See the <c>IsComplete()</c> method of class <see cref="FormattingOptions"/>.
    /// </returns>
    public bool TryGetFormattingOptions(string cpath, out FormattingOptions options)
    {
        // Start building an object to return.  Its properties are all null initially.
        FormattingOptions optionsToReturn = new();

        // Populate the object incrementally starting with the most specific FormattingOptions
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
