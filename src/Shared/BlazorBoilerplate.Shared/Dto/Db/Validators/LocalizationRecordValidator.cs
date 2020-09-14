using BlazorBoilerplate.Shared.SqlLocalizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Dto.Db.Validators
{
    public class LocalizationRecordValidator : LocalizedAbstractValidator<ILocalizationRecord>
    {
        public LocalizationRecordValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.ResourceKey)
                .NotEmpty().WithName(L["ResourceKey"]);

            RuleFor(p => p.Key)
                .NotEmpty().WithName(L["Key"]);

            RuleFor(p => p.Text)
                .NotEmpty().WithName(L["Translation"]);
        }
    }
}
