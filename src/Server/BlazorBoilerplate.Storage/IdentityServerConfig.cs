using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using Humanizer;
using IdentityModel;
using IdentityServer4.Models;
using static IdentityServer4.IdentityServerConstants;

namespace BlazorBoilerplate.Storage
{
    public class IdentityServerConfig
    {
        public const string LocalApiName = "LocalAPI";
        public const string SwaggerClientID = "swaggerui";

        // https://identityserver4.readthedocs.io/en/latest/reference/identity_resource.html
        public static readonly IEnumerable<IdentityResource> GetIdentityResources =
            new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Phone(),
                new IdentityResources.Email(),
                new IdentityResource(ScopeConstants.Roles, new List<string> { JwtClaimTypes.Role })
            };

        // https://identityserver4.readthedocs.io/en/latest/reference/api_scope.html
        public static readonly IEnumerable<ApiScope> GetApiScopes =
            new[]
            {
                new ApiScope(LocalApi.ScopeName),

                new ApiScope(LocalApiName, "My API")
            };

        // https://identityserver4.readthedocs.io/en/latest/reference/api_resource.html
        public static readonly IEnumerable<ApiResource> GetApiResources =
            new[]
            {
                new ApiResource(LocalApiName) {
                    DisplayName = LocalApiName.Humanize(LetterCasing.Title),
                    Scopes = { LocalApi.ScopeName, LocalApiName },
                    UserClaims = {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email,
                        JwtClaimTypes.EmailVerified,
                        JwtClaimTypes.Role,
                        ApplicationClaimTypes.Permission,
                        ApplicationClaimTypes.IsSubscriptionActive,
                        ApplicationClaimTypes.For(UserFeatures.User),
                        ApplicationClaimTypes.For(UserFeatures.Administrator),
                        ApplicationClaimTypes.For(UserFeatures.Operator),
                    }
                }
            };

        // https://identityserver4.readthedocs.io/en/latest/reference/client.html
        public static readonly IEnumerable<Client> GetClients =
            new[]
            {
                new Client
                {
                    ClientId = SwaggerClientID,
                    ClientName = "Swagger UI",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = true,
                    RequireClientSecret = false,
                    RequirePkce = true,

                    RedirectUris = {
                        "http://127.0.0.1:64879",
                        "http://localhost:53414/swagger/oauth2-redirect.html",
                        "https://blazor-server.quarella.net/swagger/oauth2-redirect.html" },

                    AllowedScopes = {
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email,
                        ScopeConstants.Roles,
                        LocalApi.ScopeName
                    }
                },

                new Client
                {
                    ClientClaimsPrefix = string.Empty,

                    ClientId = "com.blazorboilerplate.app",

                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = {
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email,
                        ScopeConstants.Roles,
                        LocalApi.ScopeName,
                        LocalApiName
                    },

                    RequirePkce = true,

                    RedirectUris = { "com.blazorboilerplate.app://callback" },
                    PostLogoutRedirectUris = { "com.blazorboilerplate.app://callback" },

                    AccessTokenLifetime = 300
                }
            };

    }
}
