using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; set; }
    }
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>,
        IAuthorizationRequirement

    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public PermissionRequirementHandler(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            ApplicationUser user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return;
            }

            List<Claim> roleClaims = await BuildRoleClaims(user);

            if (roleClaims.FirstOrDefault(c => c.Value == requirement.Permission) != null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        public async Task<List<Claim>> BuildRoleClaims(ApplicationUser user)
        {
            List<Claim> roleClaims = new List<Claim>();
            if (_userManager.SupportsUserRole)
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                foreach (string roleName in roles)
                {
                    if (_roleManager.SupportsRoleClaims)
                    {
                        IdentityRole<Guid> role = await _roleManager.FindByNameAsync(roleName);
                        if (role != null)
                        {
                            IList<Claim> rc = await _roleManager.GetClaimsAsync(role);
                            roleClaims.AddRange(rc.ToList());
                        }
                    }
                    roleClaims = roleClaims.Distinct(new ClaimsComparer()).ToList();
                }
            }
            return roleClaims;
        }
        public class ClaimsComparer : IEqualityComparer<Claim>
        {
            public bool Equals(Claim x, Claim y)
            {
                return x.Value == y.Value;
            }
            public int GetHashCode(Claim claim)
            {
                int claimValue = claim.Value?.GetHashCode() ?? 0;
                return claimValue;
            }
        }
    }
}
