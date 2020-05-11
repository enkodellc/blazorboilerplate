using System.Collections.Generic;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Storage.Core;
using Humanizer;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace BlazorBoilerplate.Storage
{
    public class IdentityServerConfig
    {
        public const string LocalApiName = "blazorboilerplate_api";
        public const string AppClientID = "blazorboilerplate_spa";
        public const string SwaggerClientID = "swaggerui";

        // Identity resources (used by UserInfo endpoint).
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Phone(),
                new IdentityResources.Email(),
                new IdentityResource(ScopeConstants.Roles, new List<string> { JwtClaimTypes.Role })
            };
        }

        // Api resources.
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(LocalApiName) {
                    DisplayName = LocalApiName.Humanize(LetterCasing.Title),
                    Scopes = { LocalApiName },
                    UserClaims = {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email,
                        JwtClaimTypes.PhoneNumber,
                        JwtClaimTypes.Role,
                        ClaimConstants.Permission,
                        Policies.IsUser,
                        Policies.IsAdmin
                    }
                }
            };
        }

        // Clients want to access resources.
        public static IEnumerable<IdentityServer4.Models.Client> GetClients()
        {
            // Clients credentials.
            return new List<IdentityServer4.Models.Client>
            {
                // http://docs.identityserver.io/en/release/reference/client.html.
                new Client
                {
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, // Resource Owner Password Credential grant.
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId, // For UserInfo endpoint.
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Email,
                        ScopeConstants.Roles,
                        LocalApiName
                    },
                    AllowRememberConsent = true,
                    AllowOfflineAccess = true, // For refresh token.
                    ClientId = AppClientID,
                    ClientName = AppClientID.Humanize(LetterCasing.Title),
                    ClientSecrets = new List<Secret> { new Secret { Value = "BlazorBoilerplate".Sha512() }},
                    Enabled = true,
                    RequireClientSecret = true, // This client does not need a secret to request tokens from the token endpoint.
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly
                },

                new Client
                {
                    ClientId = SwaggerClientID,
                    ClientName = "Swagger UI",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = true,
                    RequireClientSecret = false,
                    RequirePkce=true,

                    RedirectUris = {
                        "http://localhost:53414/swagger/oauth2-redirect.html",
                        "https://blazor-server.quarella.net/swagger/oauth2-redirect.html" },

                    AllowedScopes = {
                        LocalApiName
                    }
                }
            };
        }
    }
}
