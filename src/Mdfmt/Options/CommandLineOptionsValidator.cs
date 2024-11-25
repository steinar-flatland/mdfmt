using FluentValidation;

namespace Mdfmt.Options;

public class CommandLineOptionsValidator : AbstractValidator<CommandLineOptions>
{
    public CommandLineOptionsValidator()
    {
        RuleFor(o => o.Flavor).IsInEnum();
        RuleFor(o => o.HeadingNumbering).Must(HeadingNumbering.Options.Contains).
            WithMessage($"Valid options: [{string.Join(',', HeadingNumbering.Options)}]");
        RuleFor(o => o.TocThreshold).GreaterThanOrEqualTo(0);
        RuleFor(o => o.NewlineStrategy).IsInEnum();
        RuleFor(o => o.Path).NotEmpty();
    }
}
