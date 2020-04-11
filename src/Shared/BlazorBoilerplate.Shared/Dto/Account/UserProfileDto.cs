using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto.Account
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
