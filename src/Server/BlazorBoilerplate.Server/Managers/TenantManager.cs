using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Tenant;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class TenantManager : ITenantManager
    {
        private readonly ITenantStore _tenantStore;
        public readonly UserManager<ApplicationUser> _userManager;

        public TenantManager(ITenantStore tenantStore, UserManager<ApplicationUser> userManager)
        {
            _tenantStore = tenantStore;
            _userManager = userManager;
        }

        public async Task<ApiResponse> Get()
        {
            try
            {
                var tenants = await _tenantStore.GetAll();
                return new ApiResponse(Status200OK, "Retrieved Tenants", tenants);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }

        public async Task<ApiResponse> Get(string id)
        {
            try
            {
                var tenant = await _tenantStore.GetById(id);
                return new ApiResponse(Status200OK, "Retrieved Tenant", tenant);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.GetBaseException().Message);
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
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> Delete(string id)
        {
            try
            {
                Claim tenantClaim = new Claim("TenantId", id);
                var users = await _userManager.GetUsersForClaimAsync(tenantClaim);
                foreach (var user in users)
                {
                    await RemoveFromTenant(user.Id, id);
                }
                await _tenantStore.DeleteById(id);
                return new ApiResponse(Status200OK, "Soft Delete Tenant");
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> AddToTenant(Guid UserId, string TenantId)
        {
            var tenant = await _tenantStore.GetById(TenantId);
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim("TenantId", tenant.Identifier);
            if (!userClaims.Any(c => c.Type == claim.Type)) // We currently accept only one tenant claim for each user
            {
                await _userManager.AddClaimAsync(appUser, claim);
                return new ApiResponse(Status200OK, "User added to tenant");
            }
            return new ApiResponse(Status400BadRequest, "Failed to add user");
        }

        public async Task<ApiResponse> RemoveFromTenant(Guid UserId, string TenantId)
        {
            var tenant = await _tenantStore.GetById(TenantId);
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
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
    }
}