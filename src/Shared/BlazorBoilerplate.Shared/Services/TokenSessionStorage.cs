using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using Blazored.SessionStorage;

namespace BlazorBoilerplate.Shared.Services
{
    public class TokenSessionStorage : ITokenStorage
    {
        private readonly string key = nameof(Tokens);
        private readonly ISessionStorageService sessionStorage;

        public TokenSessionStorage(ISessionStorageService sessionStorage)
        {
            this.sessionStorage = sessionStorage;
        }

        public async Task<Tokens> Get(CancellationToken cancellationToken = default)
        {
            return await sessionStorage.GetItemAsync<Tokens>(key, cancellationToken);
        }

        public async Task Set(Tokens tokens, CancellationToken cancellationToken = default)
        {
            await sessionStorage.SetItemAsync(key, tokens, cancellationToken);
        }
        public async Task Clear(CancellationToken cancellationToken = default)
        {
            await sessionStorage.RemoveItemAsync(key, cancellationToken);
        }
    }
}
