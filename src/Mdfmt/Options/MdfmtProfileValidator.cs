using FluentValidation;
using FluentValidation.Results;
using System.Linq;

namespace Mdfmt.Options;

internal class MdfmtProfileValidator : AbstractValidator<MdfmtProfile>
{
    public MdfmtProfileValidator(string filePath, MdfmtProfile profile)
    {
        // Every value in the Options dictionary must pass the FileProcessingOptionsValidator.
        RuleFor(o => o.Options).Must((d) =>
        {
            return d.Values.All((f) =>
            {
                FileProcessingOptionsValidator validator = new();
                ValidationResult validationResult = validator.Validate(f);
                return validationResult.IsValid;
            });
        }).WithMessage($"Failed to validate {nameof(MdfmtProfile)} loaded from file {filePath}: The {nameof(MdfmtProfile.Options)} dictionary contains one or more values that is not a valid instance of {nameof(FileProcessingOptions)}.");

        // Every value in the CpathToOptions dictionary must be a key of the Options dictionary.
        RuleFor(o => o.CpathToOptions).Must((d) =>
        {
            return d.Values.All((v) =>
            {
                return profile.Options.ContainsKey(v);
            });
        }).WithMessage($"Failed to validate {nameof(MdfmtProfile)} loaded from file {filePath}: The {nameof(MdfmtProfile.CpathToOptions)} dictionary contains one or more value that is not a key of the {nameof(MdfmtProfile.Options)} dictionary.");
    }
}
