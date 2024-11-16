namespace Mdfmt.Model;

public class UnixNewlineRegion : NewlineRegion
{
    private UnixNewlineRegion() { }

    public override string Content => Constants.UnixNewline;

    /// <summary>
    /// Newline region containing <c>"\n"</c>.
    /// </summary>
    public static readonly UnixNewlineRegion Instance = new();
}
