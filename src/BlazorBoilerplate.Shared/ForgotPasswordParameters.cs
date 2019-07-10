using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared
{
    public class ForgotPasswordParameters
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
