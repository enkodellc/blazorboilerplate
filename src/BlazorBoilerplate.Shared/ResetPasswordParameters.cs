using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared
{
    public class ResetPasswordParameters
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Identifier too short (6 character minimum).")]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string PasswordConfirm { get; set; }
    }
}
