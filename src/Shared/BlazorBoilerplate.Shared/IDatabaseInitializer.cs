using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
    }
}