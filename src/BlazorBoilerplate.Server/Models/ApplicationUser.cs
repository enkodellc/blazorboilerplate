using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Server.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [MaxLength(64)]
        public string FirstName { get; set; }

        [MaxLength(64)]
        public string LastName { get; set; }
    }
}
