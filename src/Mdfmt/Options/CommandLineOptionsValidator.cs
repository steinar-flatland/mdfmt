using FluentValidation;

namespace Mdfmt.Options;

internal class CommandLineOptionsValidator : AbstractValidator<CommandLineOptions>
{
    public CommandLineOptionsValidator()
    {
        // In practice, this validation is useless, because the command line parser throws a CommandLine.BadFormatConversionError
        // if the value passed to -f (--flavor) is invalid.  Improve this.  It would be nice to give the user a more meaningful error.
        RuleFor(o => o.Flavor).IsInEnum();

        RuleFor(o => o.HeadingNumbering).Must(v => (v == null) || HeadingNumbering.Options.Contains(v.ToLower())).
            WithMessage($"When --heading-numbers (-h) is specified, then valid values are: [{string.Join(',', HeadingNumbering.Options)}].");

        RuleFor(o => o.Flavor).NotNull().Unless(o => o.HeadingNumbering == null).
            WithMessage($"When --heading-numbers (-h) is specified, then --flavor (-f) is required.");

        RuleFor(o => o.TocThreshold).GreaterThanOrEqualTo(0).
            WithMessage($"When --toc-threshold (-t) is specified, its value must be nonnegative.");

        RuleFor(o => o).Must(o => (o.TocThreshold == null) || (o.TocThreshold == 0) || (o.TocThreshold > 0 && o.Flavor != null)).
            WithMessage($"When --toc-threshold (-t) is 1 or more, then --flavor (-f) is required.");

        RuleFor(o => o.LineNumberingThreshold).GreaterThanOrEqualTo(0).
            WithMessage($"When --line-numbering-threshold (-l) is specified, its value must be nonnegative.");

        // In practice, this validation is useless, because the command line parser throws a CommandLine.BadFormatConversionError
        // if the value passed to --newline-strategy is invalid.  Improve this.  It would be nice to give the user a more meaningful error.
        RuleFor(o => o.NewlineStrategy).IsInEnum();

        RuleFor(o => o.TargetPath).NotEmpty();

    }
}
