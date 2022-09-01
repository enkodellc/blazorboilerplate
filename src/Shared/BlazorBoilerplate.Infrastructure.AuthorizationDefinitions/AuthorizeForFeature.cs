using Microsoft.AspNetCore.Authorization;

namespace BlazorBoilerplate.Infrastructure.AuthorizationDefinitions
{
    public class AuthorizeForFeature : AuthorizeAttribute
    {
        public AuthorizeForFeature(UserFeatures userFeature) : base($"Is{userFeature}")
        {

        }
    }
}
