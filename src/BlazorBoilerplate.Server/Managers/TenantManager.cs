using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Managers
{
    public class TenantManager : ITenantManager
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminManager _adminManager;

        public TenantManager(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IAdminManager adminManager)
        {
            _db = db;
            _userManager = userManager;
            _adminManager = adminManager;
        }

        #region Tenants

        public async Task<ApiResponse> GetTenants() => new ApiResponse(200, "Retrieved Tenants", await _db.Tenants.ToListAsync());

        public async Task<ApiResponse> GetTenant(Guid id) => new ApiResponse(200, "Retrieved Tenant", await _db.Tenants.FindAsync(id));

        public Tenant GetTenant() => _db.Tenants.Find(_db.TenantId);

        public async Task<ApiResponse> PutTenant(Tenant tenant)
        {
            Tenant t = _db.Tenants.Find(tenant.Id);
            t.Title = tenant.Title;
            try
            {
                await _db.SaveChangesAsync();
                return new ApiResponse(200, "Tenant Updated. ", t);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(tenant.Id))
                {
                    return new ApiResponse(404, "Tenant Not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<ApiResponse> PostTenant(Tenant tenant, ClaimsPrincipal User)
        {
            Tenant t = new Tenant
            {
                Title = tenant.Title
            };
            await _db.Tenants.AddAsync(t);
            await _db.SaveChangesAsync();

            // Temporarily add tenantId claim in order to create TenantManager Role for the newly created tenant
            List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimConstants.TenantId, t.Id.ToString())
                    };
            ClaimsIdentity appIdentity = new ClaimsIdentity(claims);

            User.AddIdentity(appIdentity);

            // Now, DbContext.TenantId will return the newly created tenant Id.
            // Henceforth, anything created of ITenant type, will belong to this tenant.

            RoleDto ManagerRole = new RoleDto
            {
                Name = RoleConstants.TenantManagerRoleName,
                Permissions = ApplicationPermissions.GetAllPermissionNames().ToList()
            };
            ApiResponse Result = await _adminManager.CreateRoleAsync(ManagerRole);
            if (Result.StatusCode == 200)
            {
                ApplicationUser applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
                if (await TryAddTenantClaim(applicationUser.Id, t.Id))
                {
                    IdentityResult result = await _userManager.AddToRoleAsync(applicationUser, ManagerRole.Name);
                    if (result.Succeeded)
                        return new ApiResponse(200, "Tenant Created.", t);
                }
                // Operation Failed. Rollback the changes...
                await TryRemoveTenantClaim(applicationUser.Id, t.Id);
            }
            await _adminManager.DeleteRoleAsync(ManagerRole.Name);
            _db.Tenants.Remove(t);
            await _db.SaveChangesAsync();
            return new ApiResponse(500, "Tenant Creation Failed.");
        }

        public async Task<ApiResponse> DeleteTenant(Guid id)
        {
            Claim tenantClaim = new Claim(ClaimConstants.TenantId, id.ToString());
            var users = await _userManager.GetUsersForClaimAsync(tenantClaim);
            foreach (var user in users)
            {
                await TryRemoveTenantClaim(user.Id, id);
            }
            Tenant tenant = await _db.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return new ApiResponse(404, "Tenant Not found");
            }

            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();
            return new ApiResponse(200, "Tenant Removed", tenant);
        }

        private bool TenantExists(Guid id)
        {
            return _db.Tenants.Any(e => e.Id == id);
        }

        #endregion Tenants

        #region TenantManagement

        public async Task<ApiResponse> GetTenantUsers(Guid TenantId)
        {
            Claim tenantClaim = new Claim(ClaimConstants.TenantId, TenantId.ToString());
            List<UserInfoDto> userDtoList = new List<UserInfoDto>();
            IList<ApplicationUser> listResponse;
            try
            {
                listResponse = await _userManager.GetUsersForClaimAsync(tenantClaim);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            // create the dto object with mapped properties and fetch roles associated with each user
            try
            {
                foreach (ApplicationUser applicationUser in listResponse)
                {
                    userDtoList.Add(new UserInfoDto
                    {
                        FirstName = applicationUser.FirstName,
                        LastName = applicationUser.LastName,
                        UserName = applicationUser.UserName,
                        Email = applicationUser.Email,
                        UserId = applicationUser.Id,
                        Roles = (List<string>)(await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true))
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            return new ApiResponse(200, "Tenant User list fetched", userDtoList);
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

        #endregion TenantManagement

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