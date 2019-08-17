using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Models
{
    public class UserProfile
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public string LastPageVisited { get; set; } = "/";
        public bool IsNavOpen { get; set; } = true;
        public bool IsNavMinified { get; set; } = false;
        public int Count { get; set; } = 0;
        public DateTime LastUpdatedDate { get; set; } = DateTime.MinValue;
    }
}
