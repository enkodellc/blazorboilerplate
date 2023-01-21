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
        Task<ApiResponse> GetRoles(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetRole(string name);

        Task<ApiResponse> CreateRole(RoleDto roleDto);

        Task<ApiResponse> UpdateRole([FromBody] RoleDto roleDto);

        Task<ApiResponse> DeleteRole(string name);
        #endregion

        #region Clients
        Task<ApiResponse> GetClients(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetClient(string clientId);

        Task<ApiResponse> CreateClient(ClientDto clientDto);

        Task<ApiResponse> UpdateClient([FromBody] ClientDto clientDto);

        Task<ApiResponse> DeleteClient(string clientId);
        #endregion

        #region ApiResources
        Task<ApiResponse> GetApiResources(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetApiResource(string name);

        Task<ApiResponse> CreateApiResource(ApiResourceDto apiResourceDto);

        Task<ApiResponse> UpdateApiResource([FromBody] ApiResourceDto apiResourceDto);

        Task<ApiResponse> DeleteApiResource(string name);
        #endregion

        #region IdentityResources
        Task<ApiResponse> GetIdentityResources(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetIdentityResource(string name);

        Task<ApiResponse> CreateIdentityResource(IdentityResourceDto identityResourceDto);

        Task<ApiResponse> UpdateIdentityResource([FromBody] IdentityResourceDto identityResourceDto);

        Task<ApiResponse> DeleteIdentityResource(string name);
        #endregion

        #region Tenants
        Task<ApiResponse> GetTenants(int pageSize = 0, int pageNumber = 0);

        Task<ApiResponse> GetTenant(string id);

        Task<ApiResponse> CreateTenant(TenantDto tenantDto);

        Task<ApiResponse> UpdateTenant([FromBody] TenantDto tenantDto);

        Task<ApiResponse> DeleteTenant(string id);
        #endregion
    }
}
