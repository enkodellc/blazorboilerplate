using BlazorBoilerplate.Server.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Server.Models
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