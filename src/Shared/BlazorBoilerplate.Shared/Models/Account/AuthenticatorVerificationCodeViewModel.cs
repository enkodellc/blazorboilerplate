using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class AuthenticatorVerificationCodeViewModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "VerificationCode")]
        public string Code { get; set; }
    }
}
