using System;

namespace BlazorBoilerplate.Infrastructure.AuthorizationDefinitions
{
    [Flags]
    public enum Actions
    {
        Create = 0,
        Update = 1,
        Read = 2,
        Delete = 3
    }
}
