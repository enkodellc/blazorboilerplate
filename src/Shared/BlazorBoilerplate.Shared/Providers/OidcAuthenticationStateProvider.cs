using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Shared.Providers
{
    public class OidcAuthenticationStateProvider : IdentityAuthenticationStateProvider
    {
        private readonly OidcClient _oidcClient;
        private readonly ITokenStorage _tokenStorage;
        public OidcAuthenticationStateProvider(OidcClient oidcClient, ITokenStorage tokenStorage, IAccountApiClient accountApiClient, ILogger<IdentityAuthenticationStateProvider> logger) : base(accountApiClient, logger)
        {
            _oidcClient = oidcClient;
            _tokenStorage = tokenStorage;
        }

        public async Task<ApiResponseDto> Login()
        {
            try
            {
                var response = await _oidcClient.LoginAsync();

                if (!response.IsError)
                {
                    _logger.LogInformation($"Oidc Login successful {response.AccessTokenExpiration}");

                    await _tokenStorage.Set(new Tokens
                    {
                        AccessTokenExpiration = response.AccessTokenExpiration,
                        IdentityToken = response.IdentityToken,
                        AccessToken = response.AccessToken,
                        RefreshToken = response.RefreshToken
                    });

                    return new ApiResponseDto { StatusCode = Status200OK, Result = response.User };
                }
                else
                {
                    _logger.LogError($"Oidc Login: {response.Error} - {response.ErrorDescription}");

                    return new ApiResponseDto { StatusCode = Status500InternalServerError, Message = response.ErrorDescription };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponseDto { StatusCode = Status500InternalServerError, Message = ex.GetBaseException().Message };
            }
            finally
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        public async Task<ApiResponseDto> Logout()
        {
            try
            {
                var tokens = await _tokenStorage.Get();

                var response = await _oidcClient.LogoutAsync(new LogoutRequest() { IdTokenHint = tokens.IdentityToken });

                if (!response.IsError)
                {
                    _logger.LogInformation($"Oidc Logout");

                    await _tokenStorage.Clear();

                    return new ApiResponseDto { StatusCode = Status200OK };
                }
                else
                {
                    _logger.LogError($"Oidc Logout: {response.Error} - {response.ErrorDescription}");

                    return new ApiResponseDto { StatusCode = Status500InternalServerError, Message = response.ErrorDescription };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Oidc Logout: {ex.GetBaseException().Message}");

                return new ApiResponseDto { StatusCode = Status500InternalServerError, Message = ex.GetBaseException().Message };
            }
            finally
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }
    }
}
