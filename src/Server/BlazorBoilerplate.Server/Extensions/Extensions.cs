using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if the redirect URI is for a native client.
        /// </summary>
        /// <returns></returns>
        public static bool IsNativeClient(this AuthorizationRequest context)
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
               && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
                return new Guid(user.GetSubjectId());
            else
                return new Guid();
        }
        public static string GetClientId(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
                return user.Claims.SingleOrDefault(c => c.Type == JwtClaimTypes.ClientId)?.Value;
            else
                return null;
        }
        public static string GetErrors(this IdentityResult result)
        {
            return string.Join("\n", result.Errors.Select(i => i.Description));
        }
    }
}
