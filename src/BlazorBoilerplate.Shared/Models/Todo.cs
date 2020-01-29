using System.ComponentModel.DataAnnotations;
using BlazorBoilerplate.Server.DataInterfaces;
using BlazorBoilerplate.Shared.DataInterfaces;

namespace BlazorBoilerplate.Shared.Models
{
    public class Todo : IAuditable, ISoftDelete
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}