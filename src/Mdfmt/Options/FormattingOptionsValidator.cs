﻿using FluentValidation;
using System;

namespace Mdfmt.Options;

/// <summary>
/// Basic validator for class <see cref="FormattingOptions"/>.  It only ensures that each individual
/// property is valid, but it does not worry about cross-property dependencies.  See also
/// <see cref="FormattingOptionsValidator2"/>, which also handles dependencies.
/// </summary>
internal class FormattingOptionsValidator : AbstractValidator<FormattingOptions>
{
    public FormattingOptionsValidator()
    {
        RuleFor(o => o.Flavor).Must(v => v == null || Enum.IsDefined(typeof(Flavor), v)).WithMessage(o => $"Invalid Flavor: \"{o.Flavor}\"");
        RuleFor(o => o.HeadingNumbering).Must(v => v == null || HeadingNumbering.Options.Contains(v)).WithMessage(o => $"Invalid HeadingNumbering option: \"{o.HeadingNumbering}\"");
        RuleFor(o => o.TocThreshold).Must(v => v == null || (int)v >= 0).WithMessage("TocThreshold must not be negative.");
        RuleFor(o => o.LineNumberingThreshold).Must(v => v == null || (int)v >= 0).WithMessage("LineNumberingThreshold must not be negative.");
        RuleFor(o => o.NewlineStrategy).Must(v => v == null || Enum.IsDefined(typeof(NewlineStrategy), v)).WithMessage(o => $"Invalid NewlineStrategy: \"{o.NewlineStrategy}\"");
    }
}
