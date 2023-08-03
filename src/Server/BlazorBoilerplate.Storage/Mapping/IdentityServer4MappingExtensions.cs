using IdentityServer4.EntityFramework.Entities;

namespace BlazorBoilerplate.Storage.Mapping
{
    public static class IdentityServer4MappingExtensions
    {
        #region Model To Entity

        public static ApiScope CreateEntity(this IdentityServer4.Models.ApiScope scope)
        {
            return new ApiScope()
            {
                Description = scope.Description,
                DisplayName = scope.DisplayName,
                Emphasize = scope.Emphasize,
                Enabled = scope.Enabled,
                Name = scope.Name,
                Required = scope.Required,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                Properties = scope.Properties?.Select(x => new ApiScopeProperty()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList(),
                UserClaims = scope.UserClaims?.Select(x => new ApiScopeClaim()
                {
                    Type = x,
                }).ToList()
            };
        }

        public static Client CreateEntity(this IdentityServer4.Models.Client client)
        {
            return new Client()
            {
                Description = client.Description,
                Enabled = client.Enabled,
                Properties = client.Properties?.Select(x => new ClientProperty()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList(),
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientSecrets = client.ClientSecrets?.Select(x => new ClientSecret()
                {
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Type = x.Type,
                    Value = x.Value,
                }).ToList(),
                AllowedGrantTypes = client.AllowedGrantTypes?.Select(x => new ClientGrantType()
                {
                    GrantType = x,
                }).ToList(),
                AllowedScopes = client.AllowedScopes?.Select(x => new ClientScope()
                {
                    Scope = x,
                }).ToList(),
                Claims = client.Claims?.Select(x => new ClientClaim()
                {
                    Type = x.Type,
                    Value = x.Value,
                }).ToList(),
                AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                AccessTokenLifetime = client.AccessTokenLifetime,
                DeviceCodeLifetime = client.DeviceCodeLifetime,
                ClientClaimsPrefix = client.ClientClaimsPrefix,
                ClientUri = client.ClientUri,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                IncludeJwtId = client.IncludeJwtId,
                LogoUri = client.LogoUri,
                ConsentLifetime = client.ConsentLifetime,
                AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken,
                AllowOfflineAccess = client.AllowOfflineAccess,
                AllowPlainTextPkce = client.AllowPlainTextPkce,
                AllowRememberConsent = client.AllowRememberConsent,
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                BackChannelLogoutSessionRequired = client.BackChannelLogoutSessionRequired,
                BackChannelLogoutUri = client.BackChannelLogoutUri,
                FrontChannelLogoutSessionRequired = client.FrontChannelLogoutSessionRequired,
                FrontChannelLogoutUri = client.FrontChannelLogoutUri,
                EnableLocalLogin = client.EnableLocalLogin,
                IdentityProviderRestrictions = client.IdentityProviderRestrictions?.Select(x => new ClientIdPRestriction()
                {
                    Provider = x,
                }).ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris?.Select(x => new ClientPostLogoutRedirectUri()
                {
                    PostLogoutRedirectUri = x,
                }).ToList(),
                RedirectUris = client.RedirectUris?.Select(x => new ClientRedirectUri()
                {
                    RedirectUri = x,
                }).ToList(),
                AccessTokenType = (int)client.AccessTokenType,
                RefreshTokenExpiration = (int)client.RefreshTokenExpiration,
                RefreshTokenUsage = (int)client.RefreshTokenUsage,
                RequireClientSecret = client.RequireClientSecret,
                RequireConsent = client.RequireConsent,
                RequirePkce = client.RequirePkce,
                SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                UserCodeType = client.UserCodeType,
                UserSsoLifetime = client.UserSsoLifetime,
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                ProtocolType = client.ProtocolType,
                RequireRequestObject = client.RequireRequestObject,
                PairWiseSubjectSalt = client.PairWiseSubjectSalt,
                AllowedCorsOrigins = client.AllowedCorsOrigins?.Select(x => new ClientCorsOrigin()
                {
                    Origin = x,
                }).ToList(),
                AllowedIdentityTokenSigningAlgorithms = Convert(client.AllowedIdentityTokenSigningAlgorithms),
                Created = DateTime.UtcNow,
            };
        }

        private static string Convert(ICollection<string> sourceMember)
        {
            if (sourceMember == null || !sourceMember.Any())
            {
                return null;
            }
            return sourceMember.Aggregate((x, y) => $"{x},{y}");
        }

        private static ICollection<string> Convert(string sourceMember)
        {
            var list = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(sourceMember))
            {
                sourceMember = sourceMember.Trim();
                foreach (var item in sourceMember.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static IdentityResource CreateEntity(this IdentityServer4.Models.IdentityResource identityResource)
        {
            return new IdentityResource()
            {
                Created = DateTime.UtcNow,
                Description = identityResource.Description,
                DisplayName = identityResource.DisplayName,
                Emphasize = identityResource.Emphasize,
                Enabled = identityResource.Enabled,
                Name = identityResource.Name,
                Required = identityResource.Required,
                ShowInDiscoveryDocument = identityResource.ShowInDiscoveryDocument,
                Properties = identityResource.Properties?.Select(x => new IdentityResourceProperty()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList(),
                UserClaims = identityResource.UserClaims?.Select(x => new IdentityResourceClaim()
                {
                    Type = x
                }).ToList(),
            };
        }

        public static ApiResource CreateEntity(this IdentityServer4.Models.ApiResource apiResource)
        {
            return new ApiResource()
            {
                Created = DateTime.UtcNow,
                Description = apiResource.Description,
                DisplayName = apiResource.DisplayName,
                Enabled = apiResource.Enabled,
                Name = apiResource.Name,
                ShowInDiscoveryDocument = apiResource.ShowInDiscoveryDocument,
                Properties = apiResource.Properties?.Select(x => new ApiResourceProperty()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList(),
                UserClaims = apiResource.UserClaims?.Select(x => new ApiResourceClaim()
                {
                    Type = x
                }).ToList(),
                AllowedAccessTokenSigningAlgorithms = Convert(apiResource.AllowedAccessTokenSigningAlgorithms),
                Scopes = apiResource.Scopes?.Select(x => new ApiResourceScope()
                {
                    Scope = x
                }).ToList(),
                Secrets = apiResource.ApiSecrets?.Select(x => new ApiResourceSecret()
                {
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Type = x.Type,
                    Value = x.Value,
                    Created = DateTime.UtcNow
                }).ToList(),
            };
        }

        #endregion

        #region Entity To Model

        public static IdentityServer4.Models.ApiScope CreateModel(this ApiScope apiScope)
        {
            return new IdentityServer4.Models.ApiScope()
            {
                Description = apiScope.Description,
                DisplayName = apiScope.DisplayName,
                Emphasize = apiScope.Emphasize,
                Name = apiScope.Name,
                Required = apiScope.Required,
                ShowInDiscoveryDocument = apiScope.ShowInDiscoveryDocument,
                UserClaims = apiScope.UserClaims?.Select(x => x.Type).ToList(),
                Properties = apiScope.Properties?.ToDictionary(x => x.Key, x => x.Value),
                Enabled = apiScope.Enabled,
            };
        }

        public static IdentityServer4.Models.Client CreateModel(this Client client)
        {
            return new IdentityServer4.Models.Client()
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientSecrets = client.ClientSecrets?.Select(x => new IdentityServer4.Models.Secret()
                {
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Type = x.Type,
                    Value = x.Value,
                }).ToList(),
                AllowedGrantTypes = client.AllowedGrantTypes?.Select(x => x.GrantType).ToList(),
                AllowedScopes = client.AllowedScopes?.Select(x => x.Scope).ToList(),
                Claims = client.Claims?.Select(x => new IdentityServer4.Models.ClientClaim()
                {
                    Type = x.Type,
                    Value = x.Value,
                }).ToList(),
                AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                AccessTokenLifetime = client.AccessTokenLifetime,
                DeviceCodeLifetime = client.DeviceCodeLifetime,
                ClientClaimsPrefix = client.ClientClaimsPrefix,
                ClientUri = client.ClientUri,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                IncludeJwtId = client.IncludeJwtId,
                LogoUri = client.LogoUri,
                ConsentLifetime = client.ConsentLifetime,
                AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken,
                AllowOfflineAccess = client.AllowOfflineAccess,
                AllowPlainTextPkce = client.AllowPlainTextPkce,
                AllowRememberConsent = client.AllowRememberConsent,
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                BackChannelLogoutSessionRequired = client.BackChannelLogoutSessionRequired,
                BackChannelLogoutUri = client.BackChannelLogoutUri,
                FrontChannelLogoutSessionRequired = client.FrontChannelLogoutSessionRequired,
                FrontChannelLogoutUri = client.FrontChannelLogoutUri,
                EnableLocalLogin = client.EnableLocalLogin,
                IdentityProviderRestrictions = client.IdentityProviderRestrictions?.Select(x => x.Provider).ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris?.Select(x => x.PostLogoutRedirectUri).ToList(),
                RedirectUris = client.RedirectUris?.Select(x => x.RedirectUri).ToList(),
                AccessTokenType = (IdentityServer4.Models.AccessTokenType) client.AccessTokenType,
                RefreshTokenExpiration = (IdentityServer4.Models.TokenExpiration) client.RefreshTokenExpiration,
                RefreshTokenUsage = (IdentityServer4.Models.TokenUsage) client.RefreshTokenUsage,
                RequireClientSecret = client.RequireClientSecret,
                RequireConsent = client.RequireConsent,
                RequirePkce = client.RequirePkce,
                SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                ProtocolType = client.ProtocolType,
                RequireRequestObject = client.RequireRequestObject,
                PairWiseSubjectSalt = client.PairWiseSubjectSalt,
                AllowedCorsOrigins = client.AllowedCorsOrigins?.Select(x => x.Origin).ToList(),
                AllowedIdentityTokenSigningAlgorithms = Convert(client.AllowedIdentityTokenSigningAlgorithms),
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                Properties = client.Properties?.ToDictionary(x => x.Key, x => x.Value),
                UserSsoLifetime = client.UserSsoLifetime,
                UserCodeType = client.UserCodeType,
                Enabled = client.Enabled,
                Description = client.Description
            };
        }

        public static IdentityServer4.Models.ApiResource CreateModel(this ApiResource apiResource)
        {
            return new IdentityServer4.Models.ApiResource()
            {
                Description = apiResource.Description,
                DisplayName = apiResource.DisplayName,
                Enabled = apiResource.Enabled,
                Name = apiResource.Name,
                Scopes = apiResource.Scopes?.Select(x => x.Scope).ToList(),
                UserClaims = apiResource.UserClaims?.Select(x => x.Type).ToList(),
                ApiSecrets = apiResource.Secrets?.Select(x => new IdentityServer4.Models.Secret()
                {
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Type = x.Type,
                    Value = x.Value,
                }).ToList(),
                Properties = apiResource.Properties?.ToDictionary(x => x.Key, x => x.Value),
                ShowInDiscoveryDocument = apiResource.ShowInDiscoveryDocument,
                AllowedAccessTokenSigningAlgorithms = Convert(apiResource.AllowedAccessTokenSigningAlgorithms)
            };
        }

        public static IdentityServer4.Models.IdentityResource CreateModel(this IdentityResource identityResource)
        {
            return new IdentityServer4.Models.IdentityResource()
            {
                Description = identityResource.Description,
                DisplayName = identityResource.DisplayName,
                Enabled = identityResource.Enabled,
                Name = identityResource.Name,
                UserClaims = identityResource.UserClaims?.Select(x => x.Type).ToList(),
                Properties = identityResource.Properties?.ToDictionary(x => x.Key, x => x.Value),
                Required = identityResource.Required,
                Emphasize = identityResource.Emphasize,
                ShowInDiscoveryDocument = identityResource.ShowInDiscoveryDocument
            };
        }

        #endregion
    }
}
