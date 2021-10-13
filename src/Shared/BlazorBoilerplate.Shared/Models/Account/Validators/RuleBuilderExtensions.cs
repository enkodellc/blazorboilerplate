using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.Localizer;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, IStringLocalizer<Global> l)
        {
            var options = ruleBuilder
                .NotEmpty().WithName(l["Password"])
                .MinimumLength(PasswordPolicy.RequiredLength).WithMessage(l["PasswordTooShort", PasswordPolicy.RequiredLength])
                .Matches("[A-Z]").When(p => PasswordPolicy.RequireUppercase, ApplyConditionTo.CurrentValidator).WithMessage(l["PasswordRequiresUpper"])
                .Matches("[a-z]").When(p => PasswordPolicy.RequireLowercase, ApplyConditionTo.CurrentValidator).WithMessage(l["PasswordRequiresLower"])
                .Matches("[0-9]").When(p => PasswordPolicy.RequireDigit, ApplyConditionTo.CurrentValidator).WithMessage(l["PasswordRequiresDigit"])
                .Matches("[^a-zA-Z0-9]").When(p => PasswordPolicy.RequireNonAlphanumeric, ApplyConditionTo.CurrentValidator).WithMessage(l["PasswordRequiresNonAlphanumeric"]);

            return options;
        }
    }
}
