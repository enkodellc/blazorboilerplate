using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;

namespace BlazorBoilerplate.Shared.Dto
{
    public static class IdentityServer4ModelsExtensions
    {
        private static readonly IEnumerable<string> jwtClaimTypes = typeof(JwtClaimTypes).GetFields()
            .Where(x => x.IsStatic && x.IsLiteral)
            .Select(x => x.GetValue(x).ToString());
        public static string GetDisplayValue(this Secret secret) => string.IsNullOrWhiteSpace(secret.Description) ? "No description" : secret.Description;

        public static IEnumerable<string> GetCustomUserClaims(this Resource resource) => resource.UserClaims.Except(jwtClaimTypes);

    }
}
