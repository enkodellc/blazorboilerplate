using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Localizer;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Validators.Db
{
    public class PluralTranslationValidator : LocalizedAbstractValidator<PluralTranslation>
    {
        public PluralTranslationValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.Translation)
                .NotEmpty().WithName(L["Translation"]);
        }
    }
}
