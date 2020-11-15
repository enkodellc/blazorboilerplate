﻿using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Humanizer;
using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;
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
                new ApiScope(LocalApi.ScopeName)
            };

        // https://identityserver4.readthedocs.io/en/latest/reference/api_resource.html
        public static readonly IEnumerable<ApiResource> GetApiResources =
            new[]
            {
                new ApiResource(LocalApiName) {
                    DisplayName = LocalApiName.Humanize(LetterCasing.Title),
                    Scopes = { LocalApi.ScopeName },
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
                        StandardScopes.Phone,
                        StandardScopes.Email,
                        ScopeConstants.Roles,
                        LocalApi.ScopeName
                    }
                }
            };

    }
}
