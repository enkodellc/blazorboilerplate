using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Server.Models
{
    public class UserProfile
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        public string LastPageVisited { get; set; } = "/";
        public bool IsNavOpen { get; set; } = true;
        public bool IsNavMinified { get; set; } = false;
        public int Count { get; set; } = 0;
        public DateTime LastUpdatedDate { get; set; } = DateTime.MinValue;
    }
}
