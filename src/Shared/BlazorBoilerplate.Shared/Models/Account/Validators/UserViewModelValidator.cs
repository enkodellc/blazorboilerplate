using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class UserViewModelValidator : LocalizedAbstractValidator<UserViewModel>
    {
        public UserViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.UserName)
            .NotEmpty()
            .Matches(@"^[^\s]+$").WithMessage(x => L["SpacesNotPermitted"])
            .Length(2, 64).WithName(L["UserName"]);

            RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress().WithName(L["Email"]);

            When(p => p.UserId == null, () =>
            {
                RuleFor(p => p.Password).Password(L);

                RuleFor(p => p.PasswordConfirm)
                    .Equal(p => p.Password).WithMessage(x => L["PasswordConfirmationFailed"]).WithName(L["ConfirmPassword"]);
            });

            RuleFor(p => p.FirstName)
                .MaximumLength(64).WithName(L["FirstName"]);

            RuleFor(p => p.LastName)
                .MaximumLength(64).WithName(L["LastName"]);            

            RuleFor(p => p.CompanyName)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.CompanyVatIn))
                .MaximumLength(64).WithName(L["Company"]);

            RuleFor(p => p.CompanyAddress)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.CompanyName))
                .MaximumLength(64).WithName(L["Address"]);

            RuleFor(p => p.CompanyCity)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.CompanyName))
                .MaximumLength(64).WithName(L["City"]);

            RuleFor(p => p.CompanyProvince)
                .MaximumLength(64).WithName(L["Province"]);

            RuleFor(p => p.CompanyZipCode)
                .MaximumLength(12).WithName(L["ZipCode"]);

            RuleFor(p => p.CompanyCountryCode)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.CompanyName))
                .MaximumLength(2).WithName(L["Country"]);

            //RuleFor(p => p.CompanyPhoneNumber)
            //    .NotEmpty().When(p => !string.IsNullOrEmpty(p.CompanyName))
            //    .MaximumLength(15).WithName(L["PhoneNumber"]);

            RuleFor(p => p.CompanyIntTelNumber)
                .Must(p => p == null || p.IsValid).When(p => !string.IsNullOrEmpty(p.CompanyName))
                .WithName(L["PhoneNumber"]);

            RuleFor(p => p.CompanyVatIn)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.CompanyName))
                .Matches("^[a-zA-Z0-9]+$")
                .MaximumLength(15).WithName(L["VatIn"]);
        }
    }
}