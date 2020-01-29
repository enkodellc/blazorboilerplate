using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.DataModels
{
    public class Tenant
    {
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }
    }
}
