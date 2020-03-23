using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.StatusCodes;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Server.Data.Core;

namespace BlazorBoilerplate.Server.Managers
{
    public class TenantManager : ITenantManager
    {
        private readonly ITenantStore _tenantStore;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAdminManager _adminManager;
        public TenantManager(ITenantStore tenantStore, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IAdminManager adminManager)
        {
            _tenantStore = tenantStore;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _adminManager = adminManager;
        }

        public async Task<ApiResponse> Get()
        {
            try
            {
                List<Tenant> tenants = await _tenantStore.GetAll();
                return new ApiResponse(Status200OK, "Retrieved Tenants", tenants);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }

        public async Task<ApiResponse> Get(Guid id)
        {
            try
            {
                Tenant tenant = await _tenantStore.GetById(id);
                return new ApiResponse(Status200OK, "Retrieved Tenant", tenant);
            }
            catch (Exception e)
            {
                return new ApiResponse(Status400BadRequest, "Failed to Retrieve Tenant");
            }
        }

        public async Task<ApiResponse> Create(Tenant tenant)
        {
            Tenant newTenant = await _tenantStore.Create(tenant);            
            
            // Temporarily add tenantId claim in order to create TenantManager Role for the newly created tenant
            List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimConstants.TenantId, newTenant.Id.ToString())
                    };
            ClaimsIdentity appIdentity = new ClaimsIdentity(claims);

            _httpContextAccessor.HttpContext.User.AddIdentity(appIdentity);

            // Now, DbContext.TenantId will return the newly created tenant Id.
            // Henceforth, anything created of ITenant type, will belong to this new tenant.

            RoleDto ManagerRole = new RoleDto
            {
                Name = RoleConstants.TenantManagerRoleName,
                Permissions = ApplicationPermissions.GetAllPermissionNames().ToList()
            };
            ApiResponse Result = await _adminManager.CreateRoleAsync(ManagerRole);
            if (Result.StatusCode == 200)
            {
                ApplicationUser applicationUser = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name);
                if (await TryAddTenantClaim(applicationUser.Id, newTenant.Id))
                {
                    IdentityResult result = await _userManager.AddToRoleAsync(applicationUser, ManagerRole.Name);
                    if (result.Succeeded)
                        return new ApiResponse(Status200OK, "Created Tenant", newTenant);
                }
                // Operation Failed. Rollback the changes...
                await TryRemoveTenantClaim(applicationUser.Id, newTenant.Id);
            }
            await _adminManager.DeleteRoleAsync(ManagerRole.Name);
            await _tenantStore.DeleteById(newTenant.Id);
            return new ApiResponse(500, "Tenant Creation Failed.");
        }

        public async Task<ApiResponse> Update(Tenant tenant)
        {
            try
            {
                Tenant UpdatedTenant = await _tenantStore.Update(tenant);
                return new ApiResponse(Status200OK, "Updated Tenant", UpdatedTenant);
            }
            catch (InvalidDataException dataException)
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Tenant");
            }
        }

        public async Task<ApiResponse> Delete(Guid id)
        {
            try
            {
                Claim tenantClaim = new Claim(ClaimConstants.TenantId, id.ToString());
                var users = await _userManager.GetUsersForClaimAsync(tenantClaim);
                foreach (var user in users)
                {
                    await TryRemoveTenantClaim(user.Id, id);
                }
                await _tenantStore.DeleteById(id);
                return new ApiResponse(Status200OK, "Deleted the tenant and its related claims.");
            }
            catch (InvalidDataException dataException)
            {
                return new ApiResponse(Status400BadRequest, "Failed to delete the tenant");
            }
        }

        public async Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(UserName);
            if (await TryAddTenantClaim(user.Id, TenantId))
            {
                return new ApiResponse(200, "User added as tenant user");
            }
            else
            {
                return new ApiResponse(500, "Can not add user to tenant . Maybe they are in another tenant already.");
            }
        }

        public async Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId)
        {
            if (await TryRemoveTenantClaim(UserId, TenantId))
            {
                return new ApiResponse(200, "User removed as tenant user");
            }
            else
            {
                return new ApiResponse(200, "User is not in this tenant.");
            }
        }

        private async Task<bool> TryAddTenantClaim(Guid UserId, Guid TenantId)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim(ClaimConstants.TenantId, TenantId.ToString());
            if (!userClaims.Any(c => c.Type == ClaimConstants.TenantId))//We currently accept only one tenant claim for each user (Single-level Multitenancy)
            {
                await _userManager.AddClaimAsync(appUser, claim);
                return true;
            }
            return false;
        }

        private async Task<bool> TryRemoveTenantClaim(Guid UserId, Guid TenantId)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim(ClaimConstants.TenantId, TenantId.ToString());
            if (userClaims.Any(c => c.Type == ClaimConstants.TenantId))
            {
                await _userManager.RemoveClaimAsync(appUser, claim);
                var userRoles = await _userManager.GetRolesAsync(appUser);
                await _userManager.RemoveFromRolesAsync(appUser, userRoles);
                return true;
            }
            return false;
        }
    }
}
