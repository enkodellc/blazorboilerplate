using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorBoilerplate.Server
{
    // https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims

    public class MyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>
    {
        public MyUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            identity.AddClaim(new Claim("blazor", "laser"));

            //identity.AddClaim(new Claim("ContactName", user.ContactName ?? ""));
            if (user.UserName == "nstohler")
            {
                identity.AddClaim(new Claim("hans", "wurst"));
            }
            else
            {
                identity.AddClaim(new Claim("hallo", "velo"));
            }

            return identity;
        }
    }
}
