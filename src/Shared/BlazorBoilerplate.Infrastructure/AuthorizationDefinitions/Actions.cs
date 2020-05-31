using System;

namespace BlazorBoilerplate.Infrastructure.AuthorizationDefinitions
{
    [Flags]
    public enum Actions
    {
        Create = 1,
        Update = 2,
        Read = 4,
        Delete = 8,
        CRUD= Create | Update | Read | Delete
    }
}
