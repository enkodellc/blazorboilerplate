using BlazorBoilerplate.Shared.Localizer;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class RegisterViewModelValidator : LoginInputModelValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.Email)
                .NotEmpty()
                .EmailAddress().WithName(L["Email"]);

            RuleFor(p => p.PasswordConfirm)
                .Equal(p => p.Password).WithMessage(x => L["PasswordConfirmationFailed"]).WithName(L["ConfirmPassword"]);
        }
    }
}
