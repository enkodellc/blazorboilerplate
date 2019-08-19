using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlazorBoilerplate.Shared.Dto
{
    //Use Profile field until .NET Core has aspnetprofile table / functionality is created
    public class UserProfileDto
    {        
        [Key]
        public Guid UserId { get; set; }
        public long Id { get; set; }
        [Required]
        public string LastPageVisited { get; set; } = "/";
        public bool IsNavOpen { get; set; } = true;
        public bool IsNavMinified { get; set; } = false;
        public int Count { get; set; } = 0;
    }
}
