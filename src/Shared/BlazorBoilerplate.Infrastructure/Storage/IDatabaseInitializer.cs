namespace BlazorBoilerplate.Infrastructure.Storage
{
    public interface IDatabaseInitializer
    {
        Task Seed();
        Task EnsureAdminIdentities();
    }
}