using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Storage
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
        Task EnsureAdminIdentitiesAsync();
    }
}