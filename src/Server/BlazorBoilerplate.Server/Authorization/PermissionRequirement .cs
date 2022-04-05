using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authorization;

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

    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
                return Task.CompletedTask;

            if (context.User.Claims.Any(c => c.Type == ApplicationClaimTypes.Permission && c.Value == requirement.Permission))
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.CompletedTask;
        }
    }
}
