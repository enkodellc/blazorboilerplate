using BlazorBoilerplate.Shared.DataInterfaces;
using Microsoft.AspNetCore.Identity;
using System;

namespace BlazorBoilerplate.Shared.DataModels
{
    public class ApplicationRole : IdentityRole<Guid>, ITenant
    {
        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public ApplicationRole() : base()
        {
        }
    }
}