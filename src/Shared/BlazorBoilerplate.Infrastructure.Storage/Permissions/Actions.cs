namespace BlazorBoilerplate.Infrastructure.Storage.Permissions
{
    [Flags]
    public enum Actions
    {
        Create = 1,
        Update = 2,
        Read = 4,
        Delete = 8,
        CRUD = Create | Update | Read | Delete,
        CUD = Create | Update | Delete
    }
}
