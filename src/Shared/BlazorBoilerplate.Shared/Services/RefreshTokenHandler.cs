using BlazorBoilerplate.Shared.Interfaces;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

namespace BlazorBoilerplate.Shared.Services
{
    public class RefreshTokenHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly OidcClient _oidcClient;
        protected readonly ITokenStorage _tokenStorage;
        private readonly ILogger<RefreshTokenHandler> _logger;

        private bool _disposed;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        public RefreshTokenHandler(OidcClient oidcClient, ITokenStorage tokenStorage, ILogger<RefreshTokenHandler> logger)
        {
            _oidcClient = oidcClient ?? throw new ArgumentNullException(nameof(oidcClient));
            _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));

            _logger = logger;

            InnerHandler = new SocketsHttpHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tokens = await _tokenStorage.Get(cancellationToken);

            if (tokens == null)
            {
                if (await RefreshTokensAsync(cancellationToken) == false)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = request };
                }
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation($"RefreshTokenHandler {request.RequestUri} {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            if (await RefreshTokensAsync(cancellationToken) == false)
            {
                return response;
            }

            response.Dispose(); // This 401 response will not be used for anything so is disposed to unblock the socket.

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _lock.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task<bool> RefreshTokensAsync(CancellationToken cancellationToken)
        {
            if (await _lock.WaitAsync(Timeout, cancellationToken).ConfigureAwait(false))
            {
                var tokens = await _tokenStorage.Get(cancellationToken);

                if (tokens == null)
                {
                    return false;
                }

                try
                {
                    var response = await _oidcClient.RefreshTokenAsync(tokens.RefreshToken, cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (!response.IsError)
                    {
                        tokens.AccessToken = response.AccessToken;
                        tokens.AccessTokenExpiration = response.AccessTokenExpiration;

                        if (response.RefreshToken != null)
                        {
                            tokens.RefreshToken = response.RefreshToken;
                        }

                        _logger.LogInformation($"RefreshTokens successful {response.AccessTokenExpiration}");

                        await _tokenStorage.Set(tokens, cancellationToken);

                        return true;
                    }

                    _logger.LogError($"Failed on RefreshTokensAsync: {response.Error} - {response.ErrorDescription}");
                }
                finally
                {
                    _lock.Release();
                }
            }

            return false;
        }
    }
}
