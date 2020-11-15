using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class LoginWith2faInputModel : LoginWith2faModel
    {
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }
    }
}
