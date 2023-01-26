using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Authorization
{
    public class AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly ApplicationDbContext _dbContext;
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext dbContext)
            : base(userManager, roleManager, optionsAccessor)
        {
            _dbContext = dbContext;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            //AdditionalUserClaimsPrincipalFactory needs Person to add extra claims
            await _dbContext.Entry(user).Reference(i => i.Person).LoadAsync();

            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            if (user.Person != null)
            {
                if (!string.IsNullOrWhiteSpace(user.Person.FirstName))
                {
                    identity.AddClaims(new[] { new Claim(ClaimTypes.GivenName, user.Person?.FirstName) });
                }

                if (!string.IsNullOrWhiteSpace(user.Person.LastName))
                {
                    identity.AddClaims(new[] { new Claim(ClaimTypes.Surname, user.Person?.LastName) });
                }
            }

            if (user.Person == null || user.Person.ExpirationDate == null || user.Person.ExpirationDate > DateTime.Now)
            {
                identity.AddClaims(new[] { new Claim(ApplicationClaimTypes.IsSubscriptionActive, ClaimValues.trueString) });
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
