namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface ITenantSettings<out TSettings> where TSettings : class, new()
    {
        TSettings Value { get; }
    }
}
