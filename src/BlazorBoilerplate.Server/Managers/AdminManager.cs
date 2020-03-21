using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BlazorBoilerplate.Server.Managers
{
    public class AdminManager : IAdminManager
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminManager(ApplicationDbContext db,UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
        {
            // get paginated list of users
            try
            {
                IList<ApplicationUser> listResponse;
                var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Policies.IsInTenant);
                if (authorizationResult.Succeeded)
                {
                    listResponse = await _userManager.GetUsersForClaimAsync(new Claim(ClaimConstants.TenantId, _db.TenantId.ToString()));
                }
                else
                {
                    var userList = _userManager.Users.AsQueryable();
                listResponse = userList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();
                }

                var userDtoList = new List<UserInfoDto>(); // This sucks, but Select isn't async happy, and the passing into a 'ProcessEventAsync' is another level of misdirection
                foreach (var applicationUser in listResponse)
                {
                    userDtoList.Add(new UserInfoDto
                    {
                        FirstName = applicationUser.FirstName,
                        LastName = applicationUser.LastName,
                        UserName = applicationUser.UserName,
                        Email = applicationUser.Email,
                        UserId = applicationUser.Id,
                        Roles = await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true) as List<string>
                    });
                }

                return new ApiResponse(200, "User list fetched", userDtoList);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }
        }

        public ApiResponse GetPermissions()
        {
            var permissions = ApplicationPermissions.GetAllPermissionNames();
            return new ApiResponse(200, "Permissions list fetched", permissions);
        }

        public async Task<ApiResponse> GetRoles(int pageSize = 10, int pageNumber = 0)
        {
            // get paginated list of roles
            try
            {
                var roleList = _roleManager.Roles.AsQueryable();
                var listResponse = roleList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();

                var roleDtoList = new List<RoleDto>();

                foreach (var role in listResponse)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var permissions = claims.Where(x => x.Type == "permission").Select(x => ApplicationPermissions.GetPermissionByValue(x.Value).Name).ToList();

                    roleDtoList.Add(new RoleDto
                    {
                        Name = role.Name,
                        Permissions = permissions
                    }); ;
                }

                return new ApiResponse(200, "Roles list fetched", roleDtoList);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }
        }

        public async Task<ApiResponse> GetRoleAsync(string roleName)
        {
            // get paginated list of users
            try
            {
                var identityRole = await _roleManager.FindByNameAsync(roleName);

                var claims = await _roleManager.GetClaimsAsync(identityRole);
                var permissions = claims.Where(x => x.Type == "permission").Select(x => ApplicationPermissions.GetPermissionByValue(x.Value).Name).ToList();

                var roleDto = new RoleDto
                {
                    Name = roleName,
                    Permissions = permissions
                };

                return new ApiResponse(200, "Role fetched", roleDto);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }
        }

        public async Task<ApiResponse> CreateRoleAsync(RoleDto newRole)
        {
            try
            {
                // Whenever a tenant specific role is being created, in order to avoid name conflict in AspNetRoles table, we add the tenant title to the role name.
                var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Policies.IsInTenant);
                if(authorizationResult.Succeeded)
                {
                    string TenantTitle = (await _db.Tenants.FindAsync(_db.TenantId)).Title;
                    newRole.Name = $"{TenantTitle}.{newRole.Name}";
                }
                // first make sure the role doesn't already exist
                if (_roleManager.Roles.Any(r => r.Name == newRole.Name))
                    return new ApiResponse(400, "Role already exists");

                // Create the role
                var result = await _roleManager.CreateAsync(new ApplicationRole(newRole.Name));

                if (!result.Succeeded)
                {
                    var errorMessage = result.Errors.Select(x => x.Description).Aggregate((i, j) => i + " - " + j);
                    return new ApiResponse(500, errorMessage);
                }

                // Re-create the permissions
                ApplicationRole role = await _roleManager.FindByNameAsync(newRole.Name);

                foreach (var claim in newRole.Permissions)
                {
                    var resultAddClaim = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));

                    if (!resultAddClaim.Succeeded)
                        throw new Exception("Claims Addition Failed.");
                }

                return new ApiResponse(200);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateRoleAsync(RoleDto newRole)
        {
            try
            {
                // first make sure the role already exist
                if (!_roleManager.Roles.Any(r => r.Name == newRole.Name))
                    return new ApiResponse(400, "This role doesn't exists");

                // Create the permissions
                var identityRole = await _roleManager.FindByNameAsync(newRole.Name);

                var claims = await _roleManager.GetClaimsAsync(identityRole);
                var permissions = claims.Where(x => x.Type == ClaimConstants.Permission).Select(x => x.Value).ToList();

                foreach (var permission in permissions)
                {
                    await _roleManager.RemoveClaimAsync(identityRole, new Claim(ClaimConstants.Permission, permission));
                }

                foreach (var claim in newRole.Permissions)
                {
                    var result = await _roleManager.AddClaimAsync(identityRole, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));

                    if (!result.Succeeded)
                        await _roleManager.DeleteAsync(identityRole);
                }

                return new ApiResponse(200);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteRoleAsync(string name)
        {
            try
            {
                // Check if the role is used by a user
                var users = await _userManager.GetUsersInRoleAsync(name);
                if (users.Any())
                    return new ApiResponse(404, "This role is still used by a user, you cannot delete it");

                // Delete the role
                var role = await _roleManager.FindByNameAsync(name);
                await _roleManager.DeleteAsync(role);

                return new ApiResponse(200, "Role Deletion Successful");
            }
            catch
            {
                return new ApiResponse(400, "Role Deletion Failed");
            }
        }
    }
}