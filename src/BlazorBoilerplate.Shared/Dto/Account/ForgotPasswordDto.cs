using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto
{
    public class ForgotPasswordDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
