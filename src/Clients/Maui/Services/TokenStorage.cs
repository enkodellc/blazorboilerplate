using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using Newtonsoft.Json;

namespace BlazorBoilerplateMaui.Services
{
    public class TokenStorage : ITokenStorage
    {
        private readonly string key = nameof(Tokens);
        public async Task<Tokens> Get(CancellationToken cancellationToken = default)
        {
            return JsonConvert.DeserializeObject<Tokens>(await SecureStorage.GetAsync(key) ?? string.Empty);
        }

        public async Task Set(Tokens tokens, CancellationToken cancellationToken = default)
        {
            await SecureStorage.SetAsync(key, JsonConvert.SerializeObject(tokens));
        }
        public Task Clear(CancellationToken cancellationToken = default)
        {
            SecureStorage.Remove(key);

            return Task.CompletedTask;
        }
    }
}
