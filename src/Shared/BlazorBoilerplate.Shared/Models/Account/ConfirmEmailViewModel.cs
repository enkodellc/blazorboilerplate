using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class ConfirmEmailViewModel
    {
        [Required(ErrorMessage = "FieldRequired")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        public string Token { get; set; }
    }
}
