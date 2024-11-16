namespace Mdfmt.Model;

public class CommentRegion(string content) : MutableStringRegion(content)
{
    /// <inheritdoc/>
    public override bool IsActive => false;
}
