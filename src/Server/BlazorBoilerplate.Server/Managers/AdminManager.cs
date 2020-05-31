using AutoMapper;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Core;
using Finbuckle.MultiTenant;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class AdminManager : IAdminManager
    {
        private readonly IMapper _autoMapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly TenantStoreDbContext _tenantStoreDbContext;
        private readonly IStringLocalizer<Strings> L;

        public AdminManager(IMapper autoMapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ConfigurationDbContext configurationDbContext,
            TenantStoreDbContext tenantStoreDbContext,
            IStringLocalizer<Strings> l)
        {
            _autoMapper = autoMapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _configurationDbContext = configurationDbContext;
            _tenantStoreDbContext = tenantStoreDbContext;
            L = l;
        }

        public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
        {
            try
            {
                var userList = _userManager.Users.AsQueryable();
                var count = userList.Count();
                var listResponse = userList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();

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

                return new ApiResponse(Status200OK, L["{0} users fetched", count], userDtoList);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }
        }

        public ApiResponse GetPermissions()
        {
            var permissions = ApplicationPermissions.GetAllPermissionNames();
            return new ApiResponse(Status200OK, L["Permissions list fetched"], permissions);
        }

        #region Roles
        public async Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0)
        {
            try
            {
                var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Id);
                var count = roleQuery.Count();
                var listResponse = (pageSize > 0 ? roleQuery.Skip(pageNumber * pageSize).Take(pageSize) : roleQuery).ToList();

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

                return new ApiResponse(Status200OK, L["{0} roles fetched", count], roleDtoList);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> GetRoleAsync(string roleName)
        {
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

                return new ApiResponse(Status200OK, "Role fetched", roleDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> CreateRoleAsync(RoleDto roleDto)
        {
            try
            {
                if (_roleManager.Roles.Any(r => r.Name == roleDto.Name))
                    return new ApiResponse(Status400BadRequest, L["Role {0} already exists", roleDto.Name]);

                var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleDto.Name));

                if (!result.Succeeded)
                {
                    var errorMessage = result.Errors.Select(x => x.Description).Aggregate((i, j) => i + " - " + j);
                    return new ApiResponse(Status500InternalServerError, errorMessage);
                }

                // Re-create the permissions
                var role = await _roleManager.FindByNameAsync(roleDto.Name);

                foreach (var claim in roleDto.Permissions)
                {
                    var resultAddClaim = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));

                    if (!resultAddClaim.Succeeded)
                        await _roleManager.DeleteAsync(role);
                }

                return new ApiResponse(Status200OK, L["Role {0} created", roleDto.Name], roleDto); //fix a strange System.Text.Json exception shown only in Debug_SSB 
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> UpdateRoleAsync(RoleDto roleDto)
        {
            try
            {
                var response = new ApiResponse(Status200OK, L["Role {0} updated", roleDto.Name], roleDto);

                if (!_roleManager.Roles.Any(r => r.Name == roleDto.Name))
                    response = new ApiResponse(Status400BadRequest, L["The role {0} doesn't exist", roleDto.Name]);
                else
                {
                    if (roleDto.Name == Shared.DefaultRoleNames.Administrator)
                        response = new ApiResponse(Status403Forbidden, L["Role {0} cannot be edited", roleDto.Name]);
                    else
                    {
                        // Create the permissions
                        var role = await _roleManager.FindByNameAsync(roleDto.Name);

                        var claims = await _roleManager.GetClaimsAsync(role);
                        var permissions = claims.Where(x => x.Type == ClaimConstants.Permission).Select(x => x.Value).ToList();

                        foreach (var permission in permissions)
                        {
                            await _roleManager.RemoveClaimAsync(role, new Claim(ClaimConstants.Permission, permission));
                        }

                        foreach (var claim in roleDto.Permissions)
                        {
                            var result = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));

                            if (!result.Succeeded)
                                await _roleManager.DeleteAsync(role);
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> DeleteRoleAsync(string name)
        {
            try
            {
                var response = new ApiResponse(Status200OK, L["Role {0} deleted", name]);

                // Check if the role is used by a user
                var users = await _userManager.GetUsersInRoleAsync(name);
                if (users.Any())
                    response = new ApiResponse(Status404NotFound, L["RoleInUseWarning", name]);
                else
                {
                    if (name == Shared.DefaultRoleNames.Administrator)
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
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
        #endregion

        #region Clients
        public async Task<ApiResponse> GetClientsAsync(int pageSize = 0, int pageNumber = 0)
        {
            try
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
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> GetClientAsync(string clientId)
        {
            ApiResponse response;

            try
            {
                var client = await _configurationDbContext.Clients.SingleOrDefaultAsync(i => i.ClientId == clientId);

                response = client != null ? new ApiResponse(Status200OK, "Retrieved Client", client.ToModel()) :
                                            new ApiResponse(Status404NotFound, "Failed to Retrieve Client");
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }

            return response;
        }

        public async Task<ApiResponse> CreateClientAsync(ClientDto clientDto)
        {
            try
            {
                var client = clientDto.ToEntity();
                await _configurationDbContext.Clients.AddAsync(client);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["Client {0} created", clientDto.ClientId], clientDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> UpdateClientAsync(ClientDto clientDto)
        {
            try
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
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> DeleteClientAsync(string clientId)
        {
            try
            {
                var client = _configurationDbContext.Clients.SingleOrDefault(i => i.ClientId == clientId);

                if (client == null)
                    return new ApiResponse(Status404NotFound, L["The client {0} doesn't exist", clientId]);

                _configurationDbContext.Clients.Remove(client);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["Client {0} deleted", clientId]);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
        #endregion

        #region IdentityResources
        public async Task<ApiResponse> GetIdentityResourcesAsync(int pageSize = 0, int pageNumber = 0)
        {
            try
            {
                var query = _configurationDbContext.IdentityResources
                    .Include(i => i.UserClaims)
                    .OrderBy(i => i.Id).AsQueryable();

                var count = query.Count();

                if (pageSize > 0)
                    query = query.Skip(pageNumber * pageSize).Take(pageSize);

                return new ApiResponse(Status200OK, L["{0} identity resources fetched", count], (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> GetIdentityResourceAsync(string name)
        {
            ApiResponse response;

            try
            {
                var identityResource = await _configurationDbContext.IdentityResources.SingleOrDefaultAsync(i => i.Name == name);

                response = identityResource != null ? new ApiResponse(Status200OK, "Retrieved Identity Resource", identityResource.ToModel()) :
                                            new ApiResponse(Status404NotFound, "Failed to Retrieve Identity Resource");
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }

            return response;
        }

        public async Task<ApiResponse> CreateIdentityResourceAsync(IdentityResourceDto identityResourceDto)
        {
            try
            {
                var identityResource = identityResourceDto.ToEntity();
                await _configurationDbContext.IdentityResources.AddAsync(identityResource);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["Identity Resource {0} created", identityResourceDto.Name], identityResourceDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> UpdateIdentityResourceAsync(IdentityResourceDto identityResourceDto)
        {
            try
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
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> DeleteIdentityResourceAsync(string name)
        {
            try
            {
                var identityResource = _configurationDbContext.IdentityResources.SingleOrDefault(i => i.Name == name);

                if (identityResource == null)
                    return new ApiResponse(Status404NotFound, L["The Identity resource {0} doesn't exist", name]);

                _configurationDbContext.IdentityResources.Remove(identityResource);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["Identity Resource {0} deleted", name]);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
        #endregion

        #region ApiResources
        public async Task<ApiResponse> GetApiResourcesAsync(int pageSize = 0, int pageNumber = 0)
        {
            try
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
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> GetApiResourceAsync(string name)
        {
            ApiResponse response;

            try
            {
                var apiResource = await _configurationDbContext.ApiResources.SingleOrDefaultAsync(i => i.Name == name);

                response = apiResource != null ? new ApiResponse(Status200OK, "Retrieved API Resource", apiResource.ToModel()) :
                                            new ApiResponse(Status404NotFound, "Failed to Retrieve API Resource");
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }

            return response;
        }

        public async Task<ApiResponse> CreateApiResourceAsync(ApiResourceDto apiResourceDto)
        {
            try
            {
                var apiResource = apiResourceDto.ToEntity();
                await _configurationDbContext.ApiResources.AddAsync(apiResource);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["API Resource {0} created", apiResourceDto.Name], apiResourceDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> UpdateApiResourceAsync(ApiResourceDto apiResourceDto)
        {
            try
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
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> DeleteApiResourceAsync(string name)
        {
            try
            {
                var apiResource = _configurationDbContext.ApiResources.SingleOrDefault(i => i.Name == name);

                if (apiResource == null)
                    return new ApiResponse(Status404NotFound, L["The API resource {0} doesn't exist", name]);

                _configurationDbContext.ApiResources.Remove(apiResource);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["API Resource {0} deleted", name]);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
        #endregion

        #region Tenants
        public async Task<ApiResponse> GetTenantsAsync(int pageSize = 0, int pageNumber = 0)
        {
            try
            {
                var query = _tenantStoreDbContext.TenantInfo.OrderBy(i => i.Id).AsQueryable();

                var count = query.Count();

                if (pageSize > 0)
                    query = query.Skip(pageNumber * pageSize).Take(pageSize);

                return new ApiResponse(Status200OK, L["{0} tenants fetched", count], await _autoMapper.ProjectTo<TenantDto>(query).ToListAsync());
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> GetTenantAsync(string id)
        {
            ApiResponse response;

            try
            {
                var tenant = await _tenantStoreDbContext.TenantInfo.SingleOrDefaultAsync(i => i.Id == id);

                response = tenant != null ? new ApiResponse(Status200OK, "Retrieved tenant", _autoMapper.Map<TenantDto>(tenant)) :
                                            new ApiResponse(Status404NotFound, "Failed to Retrieve Tenant");
            }
            catch (Exception ex)
            {
                response = new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }

            return response;
        }

        public async Task<ApiResponse> CreateTenantAsync(TenantDto tenantDto)
        {
            try
            {
                var tenant = _autoMapper.Map<TenantDto, TenantInfo>(tenantDto);
                await _tenantStoreDbContext.TenantInfo.AddAsync(tenant);
                await _tenantStoreDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, L["Tenant {0} created", tenantDto.Name], tenantDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> UpdateTenantAsync(TenantDto tenantDto)
        {
            try
            {
                var tenant = await _tenantStoreDbContext.TenantInfo.SingleOrDefaultAsync(i => i.Id == tenantDto.Id);

                if (tenant == null)
                    return new ApiResponse(Status400BadRequest, L["The tenant {0} doesn't exist", tenantDto.Name]);

                var response = new ApiResponse(Status200OK, L["Tenant {0} updated", tenantDto.Name], tenantDto);

                if (tenantDto.Identifier != tenant.Identifier && (tenantDto.Identifier == Shared.Settings.DefaultTenantId || tenant.Identifier == Shared.Settings.DefaultTenantId))
                    response = new ApiResponse(Status403Forbidden, L["Default Tenant identifier cannot be changed and must be unique"]);
                else
                {
                    tenant = _autoMapper.Map(tenantDto, tenant);

                    _tenantStoreDbContext.TenantInfo.Update(tenant);
                    await _tenantStoreDbContext.SaveChangesAsync();
                }

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> DeleteTenantAsync(string id)
        {
            try
            {
                var response = new ApiResponse(Status200OK, L["Tenant {0} deleted", id]);

                if (id == Shared.Settings.DefaultTenantId)
                    response = new ApiResponse(Status403Forbidden, L["Tenant {0} cannot be deleted", id]);
                else
                {
                    var tenant = await _tenantStoreDbContext.TenantInfo.SingleOrDefaultAsync(i => i.Id == id);

                    if (tenant == null)
                        return new ApiResponse(Status400BadRequest, L["The tenant {0} doesn't exist", id]);

                    Claim tenantClaim = new Claim("TenantId", id);
                    var users = await _userManager.GetUsersForClaimAsync(tenantClaim);
                    foreach (var user in users)
                    {
                        await RemoveFromTenant(user.Id, id);
                    }

                    _tenantStoreDbContext.TenantInfo.Remove(tenant);
                    await _tenantStoreDbContext.SaveChangesAsync();
                }

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> AddToTenant(Guid userId, string tenantId)
        {
            var tenant = await _tenantStoreDbContext.TenantInfo.SingleAsync(t => t.Id == tenantId);
            ApplicationUser appUser = await _userManager.FindByIdAsync(userId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim("TenantId", tenant.Identifier);
            if (!userClaims.Any(c => c.Type == claim.Type)) // We currently accept only one tenant claim for each user
            {
                await _userManager.AddClaimAsync(appUser, claim);
                return new ApiResponse(Status200OK, "User added to tenant");
            }
            return new ApiResponse(Status400BadRequest, "Failed to add user");
        }
        public async Task<ApiResponse> RemoveFromTenant(Guid userId, string tenantId)
        {
            var tenant = await _tenantStoreDbContext.TenantInfo.SingleAsync(t => t.Id == tenantId);
            ApplicationUser appUser = await _userManager.FindByIdAsync(userId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim("TenantId", tenant.Identifier);
            if (userClaims.Any(c => c.Type == claim.Type))
            {
                await _userManager.RemoveClaimAsync(appUser, claim);
                var userRoles = await _userManager.GetRolesAsync(appUser);
                await _userManager.RemoveFromRolesAsync(appUser, userRoles);
                return new ApiResponse(Status200OK, "User removed from tenant");
            }
            return new ApiResponse(Status400BadRequest, "Failed to remove user");
        }
        #endregion
    }
}
