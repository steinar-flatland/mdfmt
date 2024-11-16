namespace Mdfmt.Model;

public class WindowsNewlineRegion : NewlineRegion
{
    private WindowsNewlineRegion() { }

    public override string Content => Constants.WindowsNewline;

    /// <summary>
    /// Newline region containing <c>"\r\n"</c>.
    /// </summary>
    public static readonly WindowsNewlineRegion Instance = new();
}
