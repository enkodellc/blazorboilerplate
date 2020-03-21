using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantManager _tenantManager;
        private readonly ApiResponse _invalidModel;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TenantsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ITenantManager tenantManager)
        {
            _db = db;
            _userManager = userManager;
            _tenantManager = tenantManager;
            _invalidModel = new ApiResponse(400, "Tenant Model is Invalid");
        }

        // GET: api/Tenants
        [HttpGet]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> GetTenants() => await _tenantManager.GetTenants();

        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetTenant(Guid id) => await _tenantManager.GetTenant(id);

        [HttpGet("GetUserTenant")]
        public async Task<ApiResponse> GetUserTenant()
        {
            Claim claim = User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.TenantId);
            Guid TenantId = Guid.Empty;
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (claim != null)
            {
                TenantId = Guid.Parse(claim.Value);
            }
            return await _tenantManager.GetTenant(TenantId);
        }

        // PUT: api/Tenants/5
        [HttpPut]
        public async Task<ApiResponse> PutTenant([FromBody] Tenant tenant) => ModelState.IsValid ? await _tenantManager.PutTenant(tenant) : _invalidModel;

        // POST: api/Tenants
        [HttpPost]
        public async Task<ApiResponse> PostTenant([FromBody] Tenant tenant) => ModelState.IsValid ? await _tenantManager.PostTenant(tenant, User) : _invalidModel;

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> DeleteTenant(Guid id) => await _tenantManager.DeleteTenant(id);

        [HttpGet("Users")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> GetTenantUsers(Guid tenantId) => await _tenantManager.GetTenantUsers(_db.TenantId);

        [HttpDelete("Users/{userId}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> RemoveTenantUser(Guid tenantId, Guid userId) => await _tenantManager.RemoveTenantUser(userId, _db.TenantId);

        [HttpPost("Users/{userName}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> AddTenantUser(string userName) => await _tenantManager.AddTenantUser(userName, _db.TenantId);
    }
}