using FluentValidation;
using FluentValidation.Results;
using System.Linq;

namespace Mdfmt.Options;

/// <summary>
/// Validator of an instance of <see cref="MdfmtProfile"/>.  Validates the following:
/// <list type="number">
/// <item>
/// Each of the <see cref="FormattingOptions"/> contained in the <c>Options</c> dictionary has
/// a collection of properties where, for each property, it is either <c>null</c> or a non-<c>null</c>
/// value that is in range based on expected command line option values.
/// </item>
/// <item>
/// Each of the mappings in the <c>CpathToOptions</c> dictionary targets a key of the <c>Options</c>
/// dictionary.
/// </item>
/// </list>
/// Note that it is valid for either or both of the dictionaries to be empty or null/missing, so
/// long as this does not conflict with the validations mentioned above.  For example, it is
/// valid for both dictionaries to be missing.  It is valid (though not particularly useful) to have
/// an <c>Options</c> dictionary without a <c>CpathToOptions</c> dictionary.  But it is invalid
/// to have a populated <c>CpathToOptions</c> dictionary without an <c>Options</c> dictionary,
/// because values in <c>CpathToOptions</c> must target keys that exist in <c>Options</c>.
/// </summary>
internal class MdfmtProfileValidator : AbstractValidator<MdfmtProfile>
{
    public MdfmtProfileValidator(MdfmtProfile profile)
    {
        // Every value in the Options dictionary must be approved by the FormattingOptionsValidator.
        RuleFor(o => o.Options).Must((d) =>
        {
            return d.Values.All((f) =>
            {
                FormattingOptionsValidator validator = new();
                try
                {
                    validator.ValidateAndThrow(f);
                }
                catch (ValidationException ex)
                {
                    Output.Error($"Configuratiion validation failed: {ex.Message}");
                    throw new ExitException(ExitCodes.GeneralError);
                }
                return true;
            });
        });

        // Every value in the CpathToOptions dictionary must be a key of the Options dictionary.
        RuleFor(o => o.CpathToOptions).Must((d) =>
        {
            return d.Values.All((v) =>
            {
                if (! profile.Options.ContainsKey(v))
                {
                    Output.Error($"Configuration validation failed: The {nameof(MdfmtProfile.CpathToOptions)} dictionary contains a value that is not a key of the {nameof(MdfmtProfile.Options)} dictionary: {v}");
                    throw new ExitException(ExitCodes.GeneralError);
                }
                return true;
            });
        });
    }
}
