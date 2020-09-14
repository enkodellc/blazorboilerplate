using BlazorBoilerplate.Shared.SqlLocalizer;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Models.Account.Validators
{
    public class LoginViewModelValidator : LoginInputModelValidator<LoginViewModel>
    {
        public LoginViewModelValidator(IStringLocalizer<Global> l) : base(l)
        {
        }
    }
}
