using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared
{

    public class ConfirmEmailParameters
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
