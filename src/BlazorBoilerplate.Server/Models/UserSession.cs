using BlazorBoilerplate.Server.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Models
{
    public class UserSession : IUserSession
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int TenantId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public Dictionary<string, string> ExposedClaims { get; set; }
        public bool DisableTenantFilter { get; set; }
    }
}
