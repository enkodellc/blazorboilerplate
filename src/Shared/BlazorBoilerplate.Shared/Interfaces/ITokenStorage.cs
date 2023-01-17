using BlazorBoilerplate.Shared.Models;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface ITokenStorage
    {
        Task Set(Tokens tokens, CancellationToken cancellationToken = default);
        Task<Tokens> Get(CancellationToken cancellationToken = default);
        Task Clear(CancellationToken cancellationToken = default);
    }
}
