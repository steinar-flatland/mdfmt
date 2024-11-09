namespace Mdfmt.Options
{
    /// <summary>
    /// Option for how to manage newlines when modified Markdown files are written.
    /// </summary>
    public enum NewlineStrategy
    {
        /// <summary>
        /// Ensure <c>"\n"</c> is used as the newline.
        /// </summary>
        Unix,

        /// <summary>
        /// Ensure <c>"\r\n"</c> is the newline sequence.
        /// </summary>
        Windows,

        /// <summary>
        /// If the file being edited uses only one type of newline (whether Unix or Windows style)
        /// continue to use that.  Only if the file contains a mixture of different kinds of
        /// newlines, switch them all to <c>"\n"</c>.
        /// </summary>
        PreferUnix,

        /// <summary>
        /// If the file being edited uses only one type of newline (whether Unix or Windows style)
        /// continue to use that.  Only if the file contains a mixture of different kinds of
        /// newlines, switch them all to <c>"\r\n"</c>.
        /// </summary>
        PreferWindows
    }
}
