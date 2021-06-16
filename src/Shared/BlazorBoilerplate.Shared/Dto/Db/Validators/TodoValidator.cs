using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Dto.Db.Validators
{
    public class TodoValidator : LocalizedAbstractValidator<Todo>
    {
        public TodoValidator(IStringLocalizer<Global> l) : base(l)
        {
            RuleFor(p => p.Title)
                .NotEmpty()
                .MaximumLength(128).WithName(L["Title"]);
        }
    }
}
