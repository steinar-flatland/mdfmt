using FluentValidation;
using System;

namespace Mdfmt.Options;

internal class FileProcessingOptionsValidator : AbstractValidator<FileProcessingOptions>
{
    public FileProcessingOptionsValidator()
    {
        RuleFor(o => o.Flavor).Must((v) => { return v == null || Enum.IsDefined(typeof(Flavor), v); });
        RuleFor(o => o.HeadingNumbering.ToLower()).Must(HeadingNumbering.Options.Contains);
        RuleFor(o => o.TocThreshold).GreaterThanOrEqualTo(0);
        RuleFor(o => o.NewlineStrategy).Must((v) => { return v == null || Enum.IsDefined(typeof(NewlineStrategy), v); });
    }
}
