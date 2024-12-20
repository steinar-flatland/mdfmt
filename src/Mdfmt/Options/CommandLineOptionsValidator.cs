﻿using FluentValidation;

namespace Mdfmt.Options;

internal class CommandLineOptionsValidator : AbstractValidator<CommandLineOptions>
{
    public CommandLineOptionsValidator()
    {
        RuleFor(o => o.Flavor).IsInEnum();
        RuleFor(o => o.HeadingNumbering.ToLower()).Must(HeadingNumbering.Options.Contains).
            WithMessage($"Valid options for -h and --heading-numbers: [{string.Join(',', HeadingNumbering.Options)}]");
        RuleFor(o => o.TocThreshold).GreaterThanOrEqualTo(0);
        RuleFor(o => o.NewlineStrategy).IsInEnum();
        RuleFor(o => o.Path).NotEmpty();
    }
}
