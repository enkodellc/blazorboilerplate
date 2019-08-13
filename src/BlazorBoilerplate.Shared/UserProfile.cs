using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlazorBoilerplate.Shared
{
    //Use Profile field until .NET Core has aspnetprofile table / functionality is created
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
