using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ApplicationDbContext _context;

        public PermissionRequirementHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
                return;

            ApplicationUser user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == context.User.Identity.Name);

            if (user == null)
                return;

            var roleClaims = from ur in _context.UserRoles
                             where ur.UserId == user.Id
                             join r in _context.Roles on ur.RoleId equals r.Id
                             join rc in _context.RoleClaims on r.Id equals rc.RoleId
                             select rc;

            if (await roleClaims.AnyAsync(c => c.ClaimValue == requirement.Permission))
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}