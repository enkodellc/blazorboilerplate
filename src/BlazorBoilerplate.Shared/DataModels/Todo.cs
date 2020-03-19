using System.ComponentModel.DataAnnotations;
using BlazorBoilerplate.Shared.DataInterfaces;

namespace BlazorBoilerplate.Shared.DataModels
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