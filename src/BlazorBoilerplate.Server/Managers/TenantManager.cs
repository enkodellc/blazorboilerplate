using System;
using System.IO;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.Dto.Tenant;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class TenantManager : ITenantManager
    {
        private readonly ITenantStore _tenantStore;

        public TenantManager(ITenantStore tenantStore)
        {
            _tenantStore = tenantStore;
        }

        public async Task<ApiResponse> Get()
        {
            try
            {
                var tenants = _tenantStore.GetAll();
                return new ApiResponse(Status200OK, "Retrieved Tenants", tenants);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }

        public async Task<ApiResponse> Get(long id)
        {
            try
            {
                var tenant = _tenantStore.GetById(id);
                return new ApiResponse(Status200OK, "Retrieved Tenant", tenant);
            }
            catch (Exception e)
            {
                return new ApiResponse(Status400BadRequest, "Failed to Retrieve Tenant");
            }
        }

        public async Task<ApiResponse> Create(TenantDto tenantDto)
        {
            var tenant = await _tenantStore.Create(tenantDto);
            return new ApiResponse(Status200OK, "Created Tenant", tenant);
        }

        public async Task<ApiResponse> Update(TenantDto tenantDto)
        {
            try
            {
                var tenant = await _tenantStore.Update(tenantDto);
                return new ApiResponse(Status200OK, "Updated Tenant", tenant);
            }
            catch (InvalidDataException dataException)
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Tenant");
            }
        }

        public async Task<ApiResponse> Delete(long id)
        {
            try
            {
                await _tenantStore.DeleteById(id);
                return new ApiResponse(Status200OK, "Soft Delete Tenant");
            }
            catch (InvalidDataException dataException)
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Tenant");
            }
        }

        public Task<ApiResponse> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> Delete(int id)
        {
            throw new NotImplementedException();
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
