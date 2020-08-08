using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class LoginWith2faInputModel : LoginWith2faModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [StringLength(7, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorInvalidLength", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "AuthenticatorCode", ResourceType = typeof(Strings))]
        public string TwoFactorCode { get; set; }
    }
}
