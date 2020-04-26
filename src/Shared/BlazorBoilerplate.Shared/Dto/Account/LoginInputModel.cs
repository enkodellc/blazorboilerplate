using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto.Account
{
    public class LoginInputModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string __RequestVerificationToken { get; set; }
        public string ReturnUrl { get; set; }

        public bool IsValidReturnUrl { get; set; }
    }
}
