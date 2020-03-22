using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.DataModels
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Title { get; set; }
    }
}