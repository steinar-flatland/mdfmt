using Mdfmt.Utilities;
using System.Collections.Generic;

namespace Mdfmt.Options;

/// <summary>
/// Data loaded from .mdfmt file.
/// </summary>
public class MdfmtProfile
    (
        Dictionary<string, FileProcessingOptions> options,
        Dictionary<string, string> cpathToOptions
    )
{
    /// <summary>
    /// Dictionary keyed on a name to the associated <c>FileProcessingOptions</c>.
    /// </summary>
    public Dictionary<string, FileProcessingOptions> Options { get; } = options;

    /// <summary>
    /// Dictionary mapping a cpath to an options key that is a key to the <c>Options</c> dictionary
    /// above, indicating the <c>FileProcessingOptions</c> that apply to the cpath and its 
    /// substructure.
    /// </summary>
    public Dictionary<string, string> CpathToOptions { get; } = cpathToOptions;

    /// <summary>
    /// Try to find <c>FileProcessingOptions</c> for a cpath.
    /// </summary>
    /// <param name="cpath">
    ///   Canonical relative path of a Markdown file.
    /// </param>
    /// <param name="options">
    ///   Output parameter that is set to the <c>FileProcessingOptions</c> that were found or to
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
        options = null;
        foreach (string leftCpath in PathUtils.LeftPaths(cpath))
        {
            if (CpathToOptions.TryGetValue(leftCpath, out string optionsKey))
            {
                FileProcessingOptions moreOptions = Options[optionsKey];
                if (options == null)
                {
                    options = moreOptions;
                }
                else
                {
                    // Grab any inherited options.
                    options.FillGapsFrom(moreOptions);
                }
                if (options.IsComplete())
                    break;
            }
        }
        return options != null;
    }
}
