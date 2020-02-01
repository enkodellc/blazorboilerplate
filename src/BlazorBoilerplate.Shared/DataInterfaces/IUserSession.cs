using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.DataInterfaces
{
    public interface IUserSession
    {
        Guid UserId { get; set; }
        int TenantId { get; set; }
        List<string> Roles { get; set; }
        string UserName { get; set; }
        bool DisableTenantFilter { get; set; }
    }
}
