using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class RegisterViewModel : LoginInputModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string PasswordConfirm { get; set; }
    }
}
