using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Dto.Email.Validators
{
    public class EmailDtoValidator : LocalizedAbstractValidator<EmailDto>
    {
        public EmailDtoValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.ToAddress)
                .NotEmpty()
                .EmailAddress().WithName(L["Email"]);
        }
    }
}
