using AutoMapper;
using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Storage;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    [ApiResponseException]
    public class AdminManager : IAdminManager
    {
        private readonly IMapper _autoMapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly TenantStoreDbContext _tenantStoreDbContext;
        private readonly EntityPermissions _entityPermissions;
        private readonly ILogger<AdminManager> _logger;
        private readonly IStringLocalizer<Global> L;

        public AdminManager(IMapper autoMapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            TenantStoreDbContext tenantStoreDbContext,
            EntityPermissions entityPermissions,
            ILogger<AdminManager> logger,
            IStringLocalizer<Global> l)
        {
            _autoMapper = autoMapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantStoreDbContext = tenantStoreDbContext;
            _entityPermissions = entityPermissions;
            _logger = logger;
            L = l;
        }

        public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
        {
            var userList = _userManager.Users.AsQueryable();
            var count = userList.Count();
            var listResponse = userList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();

            var userDtoList = new List<UserViewModel>(); // This sucks, but Select isn't async happy, and the passing into a 'ProcessEventAsync' is another level of misdirection
            foreach (var applicationUser in listResponse)
            {
                userDtoList.Add(new UserViewModel
                {
                    FirstName = applicationUser.FirstName,
                    LastName = applicationUser.LastName,
                    UserName = applicationUser.UserName,
                    Email = applicationUser.Email,
                    UserId = applicationUser.Id,
                    Roles = await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true) as List<string>
                });
            }

            return new ApiResponse(Status200OK, L["{0} users fetched", count], userDtoList);
        }

        public ApiResponse GetPermissions()
        {
            var permissions = _entityPermissions.GetAllPermissionNames();
            return new ApiResponse(Status200OK, L["Permissions list fetched"], permissions);
        }

        #region Roles
        public async Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0)
        {
            var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Name);
            var count = roleQuery.Count();
            var listResponse = (pageSize > 0 ? roleQuery.Skip(pageNumber * pageSize).Take(pageSize) : roleQuery).ToList();

            var roleDtoList = new List<RoleDto>();

            foreach (var role in listResponse)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                List<string> permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission).Select(x => _entityPermissions.GetPermissionByValue(x.Value).Name).ToList();

                roleDtoList.Add(new RoleDto
                {
                    Name = role.Name,
                    Permissions = permissions
                });
            }

            return new ApiResponse(Status200OK, L["{0} roles fetched", count], roleDtoList);
        }

        public async Task<ApiResponse> GetRoleAsync(string roleName)
        {
            var identityRole = await _roleManager.FindByNameAsync(roleName);

            var claims = await _roleManager.GetClaimsAsync(identityRole);
            var permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission).Select(x => _entityPermissions.GetPermissionByValue(x.Value).Name).ToList();

            var roleDto = new RoleDto
            {
                Name = roleName,
                Permissions = permissions
            };

            return new ApiResponse(Status200OK, "Role fetched", roleDto);
        }

        public async Task<ApiResponse> CreateRoleAsync(RoleDto roleDto)
        {
            if (_roleManager.Roles.Any(r => r.Name == roleDto.Name))
                return new ApiResponse(Status400BadRequest, L["Role {0} already exists", roleDto.Name]);

            var result = await _roleManager.CreateAsync(new ApplicationRole(roleDto.Name));

            if (!result.Succeeded)
            {
                var msg = result.GetErrors();
                _logger.LogWarning($"Error while creating role {roleDto.Name}: {msg}");
                return new ApiResponse(Status400BadRequest, msg);
            }

            // Re-create the permissions
            var role = await _roleManager.FindByNameAsync(roleDto.Name);

            foreach (var claim in roleDto.Permissions)
            {
                var resultAddClaim = await _roleManager.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, _entityPermissions.GetPermissionByName(claim)));

                if (!resultAddClaim.Succeeded)
                    await _roleManager.DeleteAsync(role);
            }

            return new ApiResponse(Status200OK, L["Role {0} created", roleDto.Name], roleDto); //fix a strange System.Text.Json exception shown only in Debug_SSB
        }

        public async Task<ApiResponse> UpdateRoleAsync(RoleDto roleDto)
        {
            var response = new ApiResponse(Status200OK, L["Role {0} updated", roleDto.Name], roleDto);

            if (!_roleManager.Roles.Any(r => r.Name == roleDto.Name))
                response = new ApiResponse(Status400BadRequest, L["The role {0} doesn't exist", roleDto.Name]);
            else
            {
                if (roleDto.Name == DefaultRoleNames.Administrator)
                    response = new ApiResponse(Status403Forbidden, L["Role {0} cannot be edited", roleDto.Name]);
                else
                {
                    // Create the permissions
                    var role = await _roleManager.FindByNameAsync(roleDto.Name);

                    var claims = await _roleManager.GetClaimsAsync(role);
                    var permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission).Select(x => x.Value).ToList();

                    foreach (var permission in permissions)
                    {
                        await _roleManager.RemoveClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, permission));
                    }

                    foreach (var claim in roleDto.Permissions)
                    {
                        var result = await _roleManager.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, _entityPermissions.GetPermissionByName(claim)));

                        if (!result.Succeeded)
                            await _roleManager.DeleteAsync(role);
                    }
                }
            }

            return response;
        }

        public async Task<ApiResponse> DeleteRoleAsync(string name)
        {
            var response = new ApiResponse(Status200OK, L["Role {0} deleted", name]);

            // Check if the role is used by a user
            var users = await _userManager.GetUsersInRoleAsync(name);
            if (users.Any())
                response = new ApiResponse(Status404NotFound, L["RoleInUseWarning", name]);
            else
            {
                if (name == DefaultRoleNames.Administrator)
                    response = new ApiResponse(Status403Forbidden, L["Role {0} cannot be deleted", name]);
                else
                {
                    // Delete the role
                    var role = await _roleManager.FindByNameAsync(name);
                    await _roleManager.DeleteAsync(role);
                }
            }

            return response;
        }
        #endregion

        #region Tenants
        public async Task<ApiResponse> GetTenantsAsync(int pageSize = 0, int pageNumber = 0)
        {
            var query = _tenantStoreDbContext.TenantInfo.OrderBy(i => i.Id).AsQueryable();

            var count = query.Count();

            if (pageSize > 0)
                query = query.Skip(pageNumber * pageSize).Take(pageSize);

            return new ApiResponse(Status200OK, L["{0} tenants fetched", count], await _autoMapper.ProjectTo<TenantDto>(query).ToListAsync());
        }

        public async Task<ApiResponse> GetTenantAsync(string id)
        {
            var tenant = await _tenantStoreDbContext.TenantInfo.SingleOrDefaultAsync(i => i.Id == id);

            return tenant != null ? new ApiResponse(Status200OK, "Retrieved tenant", _autoMapper.Map<TenantDto>(tenant)) :
                                    new ApiResponse(Status404NotFound, "Failed to Retrieve Tenant");
        }

        public async Task<ApiResponse> CreateTenantAsync(TenantDto tenantDto)
        {
            var tenant = _autoMapper.Map<TenantDto, TenantInfo>(tenantDto);
            await _tenantStoreDbContext.TenantInfo.AddAsync(tenant);
            await _tenantStoreDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Tenant {0} created", tenantDto.Name], tenantDto);
        }

        public async Task<ApiResponse> UpdateTenantAsync(TenantDto tenantDto)
        {
            var tenant = await _tenantStoreDbContext.TenantInfo.SingleOrDefaultAsync(i => i.Id == tenantDto.Id);

            if (tenant == null)
                return new ApiResponse(Status400BadRequest, L["The tenant {0} doesn't exist", tenantDto.Name]);

            var response = new ApiResponse(Status200OK, L["Tenant {0} updated", tenantDto.Name], tenantDto);

            if (tenantDto.Identifier != tenant.Identifier && (tenantDto.Identifier == Constants.Settings.DefaultTenantId || tenant.Identifier == Constants.Settings.DefaultTenantId))
                response = new ApiResponse(Status403Forbidden, L["Default Tenant identifier cannot be changed and must be unique"]);
            else
            {
                tenant = _autoMapper.Map(tenantDto, tenant);

                _tenantStoreDbContext.TenantInfo.Update(tenant);
                await _tenantStoreDbContext.SaveChangesAsync();
            }

            return response;
        }

        public async Task<ApiResponse> DeleteTenantAsync(string id)
        {
            var response = new ApiResponse(Status200OK, L["Tenant {0} deleted", id]);

            if (id == Constants.Settings.DefaultTenantId)
                response = new ApiResponse(Status403Forbidden, L["Tenant {0} cannot be deleted", id]);
            else
            {
                var tenant = await _tenantStoreDbContext.TenantInfo.SingleOrDefaultAsync(i => i.Id == id);

                if (tenant == null)
                    return new ApiResponse(Status400BadRequest, L["The tenant {0} doesn't exist", id]);

                _tenantStoreDbContext.TenantInfo.Remove(tenant);
                await _tenantStoreDbContext.SaveChangesAsync();
            }

            return response;
        }
        #endregion
    }
}
