using FluentValidation;

namespace Mdfmt.Options;

/// <summary>
/// Validator for class <see cref="FormattingOptions"/>.  It adds additional rules to the
/// <see cref="FormattingOptionsValidator"/>, to also handle cross-property dependencies.
/// </summary>
internal class FormattingOptionsValidator2 : FormattingOptionsValidator
{
    public FormattingOptionsValidator2() : base()
    {
        RuleFor(o => o.Flavor).NotNull().Unless(o => o.HeadingNumbering == null).
            WithMessage($"When HeadingNumbering option is specified, then Flavor is required.");
        RuleFor(o => o).Must(o => (o.TocThreshold == null) || (o.TocThreshold == 0) || (o.TocThreshold > 0 && o.Flavor != null)).
            WithMessage($"When TocThreshold is 1 or more, then Flavor is required.");
    }
}
