using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Storage.Core;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.Mappers;

namespace BlazorBoilerplate.Server.Managers
{
    public class AdminManager : IAdminManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ConfigurationDbContext _configurationDbContext;

        public AdminManager(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager, ConfigurationDbContext configurationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configurationDbContext = configurationDbContext;
        }

        public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
        {
            // get paginated list of users
            try
            {
                var userList = _userManager.Users.AsQueryable();
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

                return new ApiResponse(Status200OK, "User list fetched", userDtoList);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }
        }

        public ApiResponse GetPermissions()
        {
            var permissions = ApplicationPermissions.GetAllPermissionNames();
            return new ApiResponse(Status200OK, "Permissions list fetched", permissions);
        }

        #region Roles
        public async Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0)
        {
            try
            {
                var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Id);
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

                return new ApiResponse(Status200OK, "Roles list fetched", roleDtoList);
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
                // first make sure the role doesn't already exist
                if (_roleManager.Roles.Any(r => r.Name == roleDto.Name))
                    return new ApiResponse(Status400BadRequest, "Role already exists");

                // Create the role
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

                return new ApiResponse(Status200OK, "Role Creation Successful", roleDto); //fix a strange System.Text.Json exception shown only in Debug_SSB 
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
                // first make sure the role already exist
                if (!_roleManager.Roles.Any(r => r.Name == roleDto.Name))
                    return new ApiResponse(Status400BadRequest, "This role doesn't exists");

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

                return new ApiResponse(Status200OK, "Role Update Successful", roleDto);
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
                // Check if the role is used by a user
                var users = await _userManager.GetUsersInRoleAsync(name);
                if (users.Any())
                    return new ApiResponse(Status404NotFound, "This role is still used by a user, you cannot delete it");

                // Delete the role
                var role = await _roleManager.FindByNameAsync(name);
                await _roleManager.DeleteAsync(role);

                return new ApiResponse(Status200OK, "Role Deletion Successful");
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

                if (pageSize > 0)
                    query = query.Skip(pageNumber * pageSize).Take(pageSize);

                return new ApiResponse(Status200OK, "Clients list fetched", (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
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

                return new ApiResponse(Status200OK, "Created Client", clientDto);
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
                    return new ApiResponse(Status400BadRequest, "This client doesn't exists");

                _configurationDbContext.Clients.Remove(client);

                _configurationDbContext.Clients.Add(clientDto.ToEntity());
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, "Client Update Successful", clientDto);
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
                    return new ApiResponse(Status404NotFound, $"Client with ClientId {clientId} does not exist");

                _configurationDbContext.Clients.Remove(client);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, "Client Deletion Successful");
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

                if (pageSize > 0)
                    query = query.Skip(pageNumber * pageSize).Take(pageSize);

                return new ApiResponse(Status200OK, "Identity Resources list fetched", (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
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

                return new ApiResponse(Status200OK, "Created Identity Resource", identityResourceDto);
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
                    return new ApiResponse(Status400BadRequest, "This Identity Resource doesn't exists");

                _configurationDbContext.IdentityResources.Remove(identityResource);

                _configurationDbContext.IdentityResources.Add(identityResourceDto.ToEntity());
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, "Identity Resource Update Successful", identityResourceDto);
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
                    return new ApiResponse(Status404NotFound, $"Identity Resource with Name {name} does not exist");

                _configurationDbContext.IdentityResources.Remove(identityResource);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, "Identity Resource Deletion Successful");
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

                if (pageSize > 0)
                    query = query.Skip(pageNumber * pageSize).Take(pageSize);

                return new ApiResponse(Status200OK, "API Resources list fetched", (await query.ToListAsync()).Select(i => i.ToModel()).ToList());
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

                return new ApiResponse(Status200OK, "Created API Resource", apiResourceDto);
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
                    return new ApiResponse(Status400BadRequest, "This API Resource doesn't exists");

                _configurationDbContext.ApiResources.Remove(apiResource);

                _configurationDbContext.ApiResources.Add(apiResourceDto.ToEntity());
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, "API Resource Update Successful", apiResourceDto);
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
                    return new ApiResponse(Status404NotFound, $"API Resource with Name {name} does not exist");

                _configurationDbContext.ApiResources.Remove(apiResource);
                await _configurationDbContext.SaveChangesAsync();

                return new ApiResponse(Status200OK, "API Resource Deletion Successful");
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
        #endregion
    }
}
