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

        #region Clients
        Task<ApiResponse> GetClientsAsync(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetClientAsync(string clientId);

        Task<ApiResponse> CreateClientAsync(ClientDto clientDto);

        Task<ApiResponse> UpdateClientAsync([FromBody] ClientDto clientDto);

        Task<ApiResponse> DeleteClientAsync(string clientId);
        #endregion

        #region ApiResources
        Task<ApiResponse> GetApiResourcesAsync(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetApiResourceAsync(string name);

        Task<ApiResponse> CreateApiResourceAsync(ApiResourceDto apiResourceDto);

        Task<ApiResponse> UpdateApiResourceAsync([FromBody] ApiResourceDto apiResourceDto);

        Task<ApiResponse> DeleteApiResourceAsync(string name);
        #endregion

        #region IdentityResources
        Task<ApiResponse> GetIdentityResourcesAsync(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetIdentityResourceAsync(string name);

        Task<ApiResponse> CreateIdentityResourceAsync(IdentityResourceDto identityResourceDto);

        Task<ApiResponse> UpdateIdentityResourceAsync([FromBody] IdentityResourceDto identityResourceDto);

        Task<ApiResponse> DeleteIdentityResourceAsync(string name);
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
