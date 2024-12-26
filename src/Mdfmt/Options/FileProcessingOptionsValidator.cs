using FluentValidation;
using System;

namespace Mdfmt.Options;

internal class FileProcessingOptionsValidator : AbstractValidator<FileProcessingOptions>
{
    public FileProcessingOptionsValidator()
    {
        RuleFor(o => o.Flavor).Must((v) => { return v == null || Enum.IsDefined(typeof(Flavor), v); });
        RuleFor(o => o.HeadingNumbering).Must((v) => { return v == null || HeadingNumbering.Options.Contains(v); });
        RuleFor(o => o.TocThreshold).Must((v) => { return v == null || (int)v >= 0; });
        RuleFor(o => o.NewlineStrategy).Must((v) => { return v == null || Enum.IsDefined(typeof(NewlineStrategy), v); });
    }
}
