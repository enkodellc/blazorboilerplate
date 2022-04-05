using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

//# Links
//## ASP.NET Core Roles/Policies/Claims
//- https://www.red-gate.com/simple-talk/dotnet/c-programming/policy-based-authorization-in-asp-net-core-a-deep-dive/
//- https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies
//- https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles
//- https://gooroo.io/GoorooTHINK/Article/17333/Custom-user-roles-and-rolebased-authorization-in-ASPNET-core/32835
//- https://gist.github.com/SteveSandersonMS/175a08dcdccb384a52ba760122cd2eda

//- (Suppress redirect on API URLs in ASP.NET Core)[https://stackoverflow.com/a/56384729/54159]
//https://adrientorris.github.io/aspnet-core/identity/extend-user-model.html

namespace BlazorBoilerplate.Server.Authorization
{
    public class AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                identity.AddClaims(new[] { new Claim(ClaimTypes.GivenName, user.FirstName) });
            }

            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                identity.AddClaims(new[] { new Claim(ClaimTypes.Surname, user.LastName) });
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                identity.AddClaims(new[] { new Claim(ClaimTypes.Email, user.Email) });
            }

            //https://docs.microsoft.com/it-it/aspnet/core/security/authentication/mfa
            if (user.TwoFactorEnabled)
            {
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, ClaimValues.AuthenticationMethodMFA));
            }
            else
            {
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, ClaimValues.AuthenticationMethodPwd));
            }

            return principal;
        }
    }
}
