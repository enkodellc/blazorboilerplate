using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IAdminManager
    {
        Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0);

        ApiResponse GetPermissions();

        Task<ApiResponse> GetRoles(int pageSize = 10,int pageNumber = 0);

        Task<ApiResponse> GetRoleAsync(string roleName);

        Task<ApiResponse> CreateRoleAsync(RoleDto newRole);

        Task<ApiResponse> UpdateRoleAsync([FromBody] RoleDto newRole);

        Task<ApiResponse> DeleteRoleAsync(string name);

    }
}
