using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public string UserId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [StringLength(100, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorInvalidLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Strings))]
        [Compare("Password", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "PasswordConfirmationFailed")]
        public string PasswordConfirm { get; set; }
    }
}
