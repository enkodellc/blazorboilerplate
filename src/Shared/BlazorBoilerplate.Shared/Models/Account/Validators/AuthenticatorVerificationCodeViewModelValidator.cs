using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class AuthenticatorVerificationCodeViewModelValidator : LocalizedAbstractValidator<AuthenticatorVerificationCodeViewModel>
    {
        public AuthenticatorVerificationCodeViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.Code)
                .NotEmpty()
                .Length(6, 7).WithName(L["VerificationCode"]);
        }
    }
}
