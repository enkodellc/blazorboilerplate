using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.DataModels
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime When { get; set; }
        public Guid UserID { get; set; }
        public ApplicationUser Sender { get; set; }
    }
}
