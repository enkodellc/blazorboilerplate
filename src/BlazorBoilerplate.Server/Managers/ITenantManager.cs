using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Managers
{
    public interface ITenantManager
    {
        Task<ApiResponse> GetTenants();

        Tenant GetTenant();

        Task<ApiResponse> GetTenant(Guid id);

        Task<ApiResponse> PostTenant(Tenant tenant, ClaimsPrincipal User);

        Task<ApiResponse> PutTenant(Tenant tenant);

        Task<ApiResponse> DeleteTenant(Guid id);

        Task<ApiResponse> GetTenantUsers(Guid TenantId);

        Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId);

        Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId);
    }
}