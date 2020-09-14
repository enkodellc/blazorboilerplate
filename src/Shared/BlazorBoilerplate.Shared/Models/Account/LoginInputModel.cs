using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class LoginInputModel : AccountFormModel
    {
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
