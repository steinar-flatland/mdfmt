using Mdfmt.Generators.Links;
using Mdfmt.Generators.Tocs;
using Mdfmt.Model;
using Mdfmt.Options;

namespace Mdfmt.Updaters;

/// <summary>
/// This class exposes methods for applying various updates to the MdStruct data structure.
/// </summary>
/// <param name="tocGenerator">Table of contents generator</param>
/// <param name="linkDestinationGenerator">Generator for link destinations, to update links in the document</param>
/// <param name="options">Command line options</param>
public class Updater(TocGenerator tocGenerator, ILinkDestinationGenerator linkDestinationGenerator, CommandLineOptions options)
{
    private readonly TocGenerator _tocGenerator = tocGenerator;
    private readonly ILinkDestinationGenerator _linkDestinationGenerator = linkDestinationGenerator;
    private readonly CommandLineOptions _options = options;

    private MdStruct _md;

    public Updater Initialize(MdStruct md)
    {
        _md = md;
        return this;
    }

    public Updater UpdateHeadingNumbers()
    {
        new HeadingNumberUpdater(_options).Update(_md);
        return this;
    }

    public Updater UpdateToc()
    {
        new TocUpdater(_options, _tocGenerator).Update(_md);
        return this;
    }

    public Updater UpdateLinks()
    {
        new LinkUpdater(_options, _linkDestinationGenerator).Update(_md);
        return this;
    }
}
