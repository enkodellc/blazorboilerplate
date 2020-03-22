using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using System;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Managers
{
    public interface ITenantManager
    {
        Task<ApiResponse> Get();

        Task<ApiResponse> Get(Guid id);

        Task<ApiResponse> Create(Tenant tenant);

        Task<ApiResponse> Update(Tenant tenant);

        Task<ApiResponse> Delete(Guid id);

        Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId);

        Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId);
    }
}
