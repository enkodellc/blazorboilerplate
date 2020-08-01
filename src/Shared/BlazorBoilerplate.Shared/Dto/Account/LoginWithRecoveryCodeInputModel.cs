using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto.Account
{
    public class LoginWithRecoveryCodeInputModel : LoginWith2faModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [DataType(DataType.Text)]
        [Display(Name = "RecoveryCode", ResourceType = typeof(Strings))]
        public string RecoveryCode { get; set; }
    }
}
