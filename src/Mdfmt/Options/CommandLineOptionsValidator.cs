using FluentValidation;

namespace Mdfmt.Options;

internal class CommandLineOptionsValidator : AbstractValidator<CommandLineOptions>
{
    public CommandLineOptionsValidator()
    {
        RuleFor(o => o.Flavor).IsInEnum().Unless(o => o == null);
        RuleFor(o => o.HeadingNumbering).Must(o => (o == null) || HeadingNumbering.Options.Contains(o.ToLower())).
            WithMessage($"Valid options for -h (--heading-numbers): [{string.Join(',', HeadingNumbering.Options)}]");
        RuleFor(o => o).Must(o => (o.TocThreshold == null) || (o.TocThreshold == 0) || (o.TocThreshold > 0 && o.Flavor != null)).
            WithMessage($"When specified, -t must be >= 0.  When -t > 0, -f is required.");
        RuleFor(o => o.LineNumberingThreshold).GreaterThanOrEqualTo(0).Unless(o => o == null).
            WithMessage($"When specified, -l must be >= 0.");
        RuleFor(o => o.NewlineStrategy).IsInEnum().Unless(o => o == null);
        RuleFor(o => o.TargetPath).NotEmpty();
    }
}
