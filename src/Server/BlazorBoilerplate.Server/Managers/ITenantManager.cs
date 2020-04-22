using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto.Tenant;
using System;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Managers
{
    public interface ITenantManager
    {
        Task<ApiResponse> Get();

        Task<ApiResponse> Get(string id);

        Task<ApiResponse> Create(TenantDto tenant);

        Task<ApiResponse> Update(TenantDto tenant);

        Task<ApiResponse> Delete(string id);

        Task<ApiResponse> AddToTenant(Guid UserId, string TenantId);

        Task<ApiResponse> RemoveFromTenant(Guid UserId, string TenantId);
    }
}