using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [MultiTenant]
    [Permissions(Actions.CRUD)]
    public partial class ApplicationUser : IdentityUser<Guid>
    {
        [MaxLength(64)]
        public string FirstName { get; set; }

        [MaxLength(64)]
        public string LastName { get; set; }

        [MaxLength(64)]
        public string FullName { get; set; }

        [JsonIgnore]
        public override string PasswordHash { get; set; }

        [JsonIgnore]
        public override string SecurityStamp { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        public ICollection<ApiLogItem> ApiLogItems { get; set; }

        public UserProfile Profile { get; set; }

        public virtual ICollection<Message> Messages { get; set; }        
    }
}