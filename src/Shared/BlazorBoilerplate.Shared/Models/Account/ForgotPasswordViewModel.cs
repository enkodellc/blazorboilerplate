using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
