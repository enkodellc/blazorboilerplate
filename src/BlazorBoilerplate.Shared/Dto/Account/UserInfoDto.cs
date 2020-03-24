using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.Account
{
    public class UserInfoDto : ICloneable
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int TenantId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public List<KeyValuePair<string, string>> ExposedClaims { get; set; }
        public bool DisableTenantFilter { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public UserInfoDto DeepClone()
        {
            return new UserInfoDto
            {
                UserName = this.UserName,
                UserId = this.UserId,
                Email = this.Email,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Roles = this.Roles
            };
        }
    }
}
