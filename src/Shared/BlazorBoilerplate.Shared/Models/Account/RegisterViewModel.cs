using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class RegisterViewModel : LoginInputModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "EmailInvalid")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Strings))]
        [Compare("Password", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "PasswordConfirmationFailed")]
        public string PasswordConfirm { get; set; }
    }
}
