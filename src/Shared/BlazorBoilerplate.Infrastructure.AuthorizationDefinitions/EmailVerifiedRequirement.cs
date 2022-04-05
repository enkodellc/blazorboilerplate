using IdentityModel;
using Microsoft.AspNetCore.Authorization;

namespace BlazorBoilerplate.Infrastructure.AuthorizationDefinitions
{
    public class EmailVerifiedRequirement : IAuthorizationRequirement
    {
        public bool IsEmailVerified { get; private set; } //not used

        public EmailVerifiedRequirement(bool isEmailVerified)
        {
            IsEmailVerified = isEmailVerified;
        }
    }

    public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            EmailVerifiedRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == JwtClaimTypes.EmailVerified))
            {
                var claim = context.User.FindFirst(c => c.Type == JwtClaimTypes.EmailVerified);
                var isEmailVerified = Convert.ToBoolean(claim.Value);

                if (isEmailVerified)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
