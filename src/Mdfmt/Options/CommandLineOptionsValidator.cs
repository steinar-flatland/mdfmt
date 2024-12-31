using FluentValidation;

namespace Mdfmt.Options;

internal class CommandLineOptionsValidator : AbstractValidator<CommandLineOptions>
{
    public CommandLineOptionsValidator()
    {
        RuleFor(o => o.Flavor).IsInEnum();
        RuleFor(o => o.HeadingNumbering).Must(o => o == null || HeadingNumbering.Options.Contains(o.ToLower())).
            WithMessage($"Valid options for -h and --heading-numbers: [{string.Join(',', HeadingNumbering.Options)}]");
        RuleFor(o => o.TocThreshold).GreaterThanOrEqualTo(0);
        RuleFor(o => o.NewlineStrategy).IsInEnum();
        RuleFor(o => o.Path).NotEmpty();
    }
}
