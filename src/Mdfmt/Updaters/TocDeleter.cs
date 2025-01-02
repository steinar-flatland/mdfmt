using Mdfmt.Model;

namespace Mdfmt.Updaters;

/// <summary>
/// Used when you want to ensure that a Markdown document does not have a TOC.
/// </summary>
internal static class TocDeleter
{
    /// <summary>
    /// Ensure that a Markdown document does not have a TOC.
    /// </summary>
    /// <param name="md">
    /// A Markdown file loaded into <see cref="MdStruct"/>.
    /// </param>
    /// <param name="verbose">
    /// Whether to generate verbose output.
    /// </param>
    public static void DeleteToc(MdStruct md, bool verbose)
    {
        if (md.HasToc)
        {
            md.DeleteToc();
            if (verbose)
            {
                Output.Emphasis("Removed TOC");
            }
        }
    }
}
