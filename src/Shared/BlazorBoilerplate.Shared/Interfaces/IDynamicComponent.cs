namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IDynamicComponent
    {
        string IntoComponent { get; }
        int Order { get; }
    }
}
