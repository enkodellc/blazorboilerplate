using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Server.Models
{
    public class Tenant
    {
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }
    }
}
