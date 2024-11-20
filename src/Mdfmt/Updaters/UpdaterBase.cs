using Mdfmt.Model;
using Mdfmt.Options;

namespace Mdfmt.Updaters;

/// <summary>
/// Abstract base class for a thing that knows how to apply some kind of update to the MdStruct.
/// </summary>
public abstract class UpdaterBase(CommandLineOptions options)
{
    protected readonly CommandLineOptions _options = options;

    public abstract void Update(MdStruct md);
}
