using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class UpdatePasswordViewModelValidator : LocalizedAbstractValidator<UpdatePasswordViewModel>
    {
        public UpdatePasswordViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.NewPassword).Password(L).WithName(L["NewPassword"]);

            RuleFor(p => p.NewPasswordConfirm)
                .Equal(p => p.NewPassword).WithMessage(x => L["PasswordConfirmationFailed"]).WithName(L["ConfirmNewPassword"]);

            RuleFor(p => p.CurrentPassword)
                .NotEmpty().WithName(L["CurrentPassword"]);
        }
    }
}
