using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "EmailInvalid")]
        public string Email { get; set; }
    }
}
