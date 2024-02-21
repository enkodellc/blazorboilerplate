using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IAdminManager
    {
        Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0);

        ApiResponse GetPermissions();

        #region Roles
        Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetRoleAsync(string name);

        Task<ApiResponse> CreateRoleAsync(RoleDto roleDto);

        Task<ApiResponse> UpdateRoleAsync([FromBody] RoleDto roleDto);

        Task<ApiResponse> DeleteRoleAsync(string name);
        #endregion

        #region Tenants
        Task<ApiResponse> GetTenantsAsync(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetTenantAsync(string id);

        Task<ApiResponse> CreateTenantAsync(TenantDto tenantDto);

        Task<ApiResponse> UpdateTenantAsync([FromBody] TenantDto tenantDto);

        Task<ApiResponse> DeleteTenantAsync(string id);
        #endregion
    }
}
