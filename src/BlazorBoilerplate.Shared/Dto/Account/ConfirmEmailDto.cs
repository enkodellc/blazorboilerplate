using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto
{

    public class ConfirmEmailDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
