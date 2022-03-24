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
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
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
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly TenantStoreDbContext _tenantStoreDbContext;
        private readonly EntityPermissions _entityPermissions;
        private readonly ILogger<AdminManager> _logger;
        private readonly IStringLocalizer<Global> L;

        public AdminManager(IMapper autoMapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ConfigurationDbContext configurationDbContext,
            TenantStoreDbContext tenantStoreDbContext,
            EntityPermissions entityPermissions,
            ILogger<AdminManager> logger,
            IStringLocalizer<Global> l)
        {
            _autoMapper = autoMapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _configurationDbContext = configurationDbContext;
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

        #region Clients
        public async Task<ApiResponse> GetClientsAsync(int pageSize = 0, int pageNumber = 0)
        {
            var query = _configurationDbContext.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.ClientSecrets)
                .Include(c => c.AllowedScopes)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.IdentityProviderRestrictions)
                .OrderBy(i => i.ClientId).AsQueryable();

            var count = query.Count();

            if (pageSize > 0)
                query = query.Skip(pageNumber * pageSize).Take(pageSize);

            return new ApiResponse(Status200OK, L["{0} clients fetched", count], (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
        }

        public async Task<ApiResponse> GetClientAsync(string clientId)
        {
            var client = await _configurationDbContext.Clients.SingleOrDefaultAsync(i => i.ClientId == clientId);

            return client != null ? new ApiResponse(Status200OK, "Retrieved Client", client.ToModel()) :
                                    new ApiResponse(Status404NotFound, "Failed to Retrieve Client");
        }

        public async Task<ApiResponse> CreateClientAsync(ClientDto clientDto)
        {
            var client = clientDto.ToEntity();
            await _configurationDbContext.Clients.AddAsync(client);
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Client {0} created", clientDto.ClientId], clientDto);
        }

        public async Task<ApiResponse> UpdateClientAsync(ClientDto clientDto)
        {
            //ClientId is not the primary key, but a unique index and ClientDto does not contain the real key Id.
            //So in UI I have to use ClientId as a key and I make it read only.
            var client = await _configurationDbContext.Clients.SingleOrDefaultAsync(i => i.ClientId == clientDto.ClientId);

            if (client == null)
                return new ApiResponse(Status400BadRequest, L["The client {0} doesn't exist", clientDto.ClientId]);

            _configurationDbContext.Clients.Remove(client);

            _configurationDbContext.Clients.Add(clientDto.ToEntity());
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Client {0} updated", clientDto.ClientId], clientDto);
        }

        public async Task<ApiResponse> DeleteClientAsync(string clientId)
        {
            var client = _configurationDbContext.Clients.SingleOrDefault(i => i.ClientId == clientId);

            if (client == null)
                return new ApiResponse(Status404NotFound, L["The client {0} doesn't exist", clientId]);

            _configurationDbContext.Clients.Remove(client);
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Client {0} deleted", clientId]);
        }
        #endregion

        #region IdentityResources
        public async Task<ApiResponse> GetIdentityResourcesAsync(int pageSize = 0, int pageNumber = 0)
        {
            var query = _configurationDbContext.IdentityResources
                .Include(i => i.UserClaims)
                .OrderBy(i => i.Id).AsQueryable();

            var count = query.Count();

            if (pageSize > 0)
                query = query.Skip(pageNumber * pageSize).Take(pageSize);

            return new ApiResponse(Status200OK, L["{0} identity resources fetched", count], (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
        }

        public async Task<ApiResponse> GetIdentityResourceAsync(string name)
        {
            var identityResource = await _configurationDbContext.IdentityResources.SingleOrDefaultAsync(i => i.Name == name);

            return identityResource != null ? new ApiResponse(Status200OK, "Retrieved Identity Resource", identityResource.ToModel()) :
                                              new ApiResponse(Status404NotFound, "Failed to Retrieve Identity Resource");
        }

        public async Task<ApiResponse> CreateIdentityResourceAsync(IdentityResourceDto identityResourceDto)
        {
            var identityResource = identityResourceDto.ToEntity();
            await _configurationDbContext.IdentityResources.AddAsync(identityResource);
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Identity Resource {0} created", identityResourceDto.Name], identityResourceDto);
        }

        public async Task<ApiResponse> UpdateIdentityResourceAsync(IdentityResourceDto identityResourceDto)
        {
            //Name is not the primary key, but a unique index and IdentityResourceDto does not contain the real key Id.
            //So in UI I have to use Name as a key and I make it read only.
            var identityResource = await _configurationDbContext.IdentityResources.SingleOrDefaultAsync(i => i.Name == identityResourceDto.Name);

            if (identityResource == null)
                return new ApiResponse(Status400BadRequest, L["The Identity resource {0} doesn't exist", identityResourceDto.Name]);

            _configurationDbContext.IdentityResources.Remove(identityResource);

            _configurationDbContext.IdentityResources.Add(identityResourceDto.ToEntity());
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Identity Resource {0} updated", identityResourceDto.Name], identityResourceDto);
        }

        public async Task<ApiResponse> DeleteIdentityResourceAsync(string name)
        {
            var identityResource = _configurationDbContext.IdentityResources.SingleOrDefault(i => i.Name == name);

            if (identityResource == null)
                return new ApiResponse(Status404NotFound, L["The Identity resource {0} doesn't exist", name]);

            _configurationDbContext.IdentityResources.Remove(identityResource);
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["Identity Resource {0} deleted", name]);
        }
        #endregion

        #region ApiResources
        public async Task<ApiResponse> GetApiResourcesAsync(int pageSize = 0, int pageNumber = 0)
        {
            var query = _configurationDbContext.ApiResources
                .Include(i => i.Secrets)
                .Include(i => i.Scopes)
                .Include(i => i.UserClaims)
                .OrderBy(i => i.Id).AsQueryable();

            var count = query.Count();

            if (pageSize > 0)
                query = query.Skip(pageNumber * pageSize).Take(pageSize);

            return new ApiResponse(Status200OK, L["{0} API resources fetched", count], (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
        }

        public async Task<ApiResponse> GetApiResourceAsync(string name)
        {
            var apiResource = await _configurationDbContext.ApiResources.SingleOrDefaultAsync(i => i.Name == name);

            return apiResource != null ? new ApiResponse(Status200OK, "Retrieved API Resource", apiResource.ToModel()) :
                                         new ApiResponse(Status404NotFound, "Failed to Retrieve API Resource");
        }

        public async Task<ApiResponse> CreateApiResourceAsync(ApiResourceDto apiResourceDto)
        {
            var apiResource = apiResourceDto.ToEntity();
            await _configurationDbContext.ApiResources.AddAsync(apiResource);
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["API Resource {0} created", apiResourceDto.Name], apiResourceDto);
        }

        public async Task<ApiResponse> UpdateApiResourceAsync(ApiResourceDto apiResourceDto)
        {
            //Name is not the primary key, but a unique index and ApiResourceDto does not contain the real key Id.
            //So in UI I have to use Name as a key and I make it read only.
            var apiResource = await _configurationDbContext.ApiResources.SingleOrDefaultAsync(i => i.Name == apiResourceDto.Name);

            if (apiResource == null)
                return new ApiResponse(Status400BadRequest, L["The API resource {0} doesn't exist", apiResourceDto.Name]);

            _configurationDbContext.ApiResources.Remove(apiResource);

            _configurationDbContext.ApiResources.Add(apiResourceDto.ToEntity());
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["API Resource {0} updated", apiResourceDto.Name], apiResourceDto);
        }

        public async Task<ApiResponse> DeleteApiResourceAsync(string name)
        {
            var apiResource = _configurationDbContext.ApiResources.SingleOrDefault(i => i.Name == name);

            if (apiResource == null)
                return new ApiResponse(Status404NotFound, L["The API resource {0} doesn't exist", name]);

            _configurationDbContext.ApiResources.Remove(apiResource);
            await _configurationDbContext.SaveChangesAsync();

            return new ApiResponse(Status200OK, L["API Resource {0} deleted", name]);
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
