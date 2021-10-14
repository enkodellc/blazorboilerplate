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

            RuleFor(p => p.Password).Password(L).WithName(L["NewPassword"]);

            RuleFor(p => p.PasswordConfirm)
                .Equal(p => p.Password).WithMessage(x => L["PasswordConfirmationFailed"]).WithName(L["ConfirmNewPassword"]);
        }
    }

    public class ChangePasswordViewModelValidator : ChangePasswordViewModelValidator<ChangePasswordViewModel>
    {
        public ChangePasswordViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {

        }
    }
}
