using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class ChangePasswordViewModelValidator<T> : LocalizedAbstractValidator<T> where T : ChangePasswordViewModel
    {
        public ChangePasswordViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.UserId)
                .NotEmpty();

            RuleFor(p => p.Password)
                .NotEmpty()
                .Length(6, 100).WithName(L["Password"]);

            RuleFor(p => p.PasswordConfirm)
                .Equal(p => p.Password).WithMessage(x => L["PasswordConfirmationFailed"]).WithName(L["ConfirmPassword"]);
        }
    }
}
