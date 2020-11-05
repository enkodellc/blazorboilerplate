using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class LoginWith2faInputModelValidator : LocalizedAbstractValidator<LoginWith2faInputModel>
    {
        public LoginWith2faInputModelValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.TwoFactorCode)
                .NotEmpty()
                .Length(6, 7).WithName(L["AuthenticatorCode"]);
        }
    }
}
