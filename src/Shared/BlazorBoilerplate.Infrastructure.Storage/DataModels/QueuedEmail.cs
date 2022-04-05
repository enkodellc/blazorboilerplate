using BlazorBoilerplate.Constants;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    public class QueuedEmail
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Email { get; set; }
        public EmailType EmailType { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? SentOn { get; set; }
    }
}
