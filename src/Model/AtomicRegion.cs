using System;

namespace Mdfmt.Model
{
    /// <summary>
    /// An atomic region of Markdown content.  "Atomic" means that the content is as it is here, and
    /// it is not modeled further through subregions.  Related class: <see cref="CompositeRegion"/>.
    /// </summary>
    internal class AtomicRegion(string content, bool isMutable, bool isHeading = false, bool isToc = false) : Region(isHeading, isToc)
    {
        /// <summary>
        /// The single string of content of this atomic region.  Backing storage for <c>Content</c> property.
        /// </summary>
        private string _content = content;

        /// <inheritdoc/>
        public override string Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (!IsMutable)
                {
                    throw new InvalidOperationException($"Attempt to change an immutable {nameof(AtomicRegion)}.");
                }
                // Once a modified state has occurred, there's no way to go back to unmodified.  If
                // not modified yet, then transition to a modified state will occur only when some
                // content is assigned, that is different from the existing content.  This prevents
                // entering a modified state for a frivolous reason.
                IsModified = IsModified || value != _content;
                _content = value;
            }
        }

        /// <inheritdoc/>
        public override bool IsMutable { get; } = isMutable;

        /// <inheritdoc/>
        public override string MutableContent => IsMutable ? _content : string.Empty;

        /// <inheritdoc/>
        public override bool IsModified { get; set; } // initially false
    }
}
