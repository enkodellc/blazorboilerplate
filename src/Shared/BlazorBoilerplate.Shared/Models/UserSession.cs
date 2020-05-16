using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Models
{
    public class UserSession : IUserSession
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public List<KeyValuePair<string, string>> ExposedClaims { get; set; }
        public bool DisableTenantFilter { get; set; } = false;

        public UserSession() 
        {
        }

        public UserSession(ApplicationUser user)
        {
            UserId = user.Id;
            UserName = user.UserName;
        }
    }
}
