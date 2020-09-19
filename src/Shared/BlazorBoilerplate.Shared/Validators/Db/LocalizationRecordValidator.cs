using BlazorBoilerplate.Shared.Interfaces.Db;
using BlazorBoilerplate.Shared.SqlLocalizer;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Validators.Db
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
