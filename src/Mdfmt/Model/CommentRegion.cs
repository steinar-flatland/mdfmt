﻿namespace Mdfmt.Model;

internal class CommentRegion(string content) : MutableStringRegion(content)
{
    /// <inheritdoc/>
    public override bool IsActive => false;
}
