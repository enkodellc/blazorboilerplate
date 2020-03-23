using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantManager _tenantManager;

        public TenantsController(ApplicationDbContext db, ITenantManager tenantManager)
        {
            _db = db;
            _tenantManager = tenantManager;
        }

        // GET: api/Tenants
        [HttpGet]
        public async Task<ApiResponse> Get()
            => await _tenantManager.Get();

        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> Get(Guid id)
            => ModelState.IsValid ?
                await _tenantManager.Get(id) :
                new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // POST: api/Tenants
        [HttpPost]
        public async Task<ApiResponse> Post([FromBody] Tenant tenant)
            => ModelState.IsValid ?
            await _tenantManager.Create(tenant) :
            new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        public async Task<ApiResponse> Put([FromBody] Tenant tenant)
            => ModelState.IsValid ? await _tenantManager.Update(tenant)
            : new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        public async Task<ApiResponse> Delete(Guid id)
            => await _tenantManager.Delete(id);

        [HttpDelete("RemoveUser/{userId}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> RemoveTenantUser(Guid userId) => await _tenantManager.RemoveTenantUser(userId, _db.TenantId);

        [HttpPost("AddUser/{userName}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> AddTenantUser(string userName) => await _tenantManager.AddTenantUser(userName, _db.TenantId);

        [HttpDelete("RemoveUser/{tenantId}/{userId}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> RemoveTenantUser(Guid tenantId, Guid userId) => await _tenantManager.RemoveTenantUser(userId, tenantId);

        [HttpPost("AddUser/{tenantId}/{userName}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> AddTenantUser(Guid tenantId, string userName) => await _tenantManager.AddTenantUser(userName, tenantId);

        [HttpGet("Current")]
        public async Task<ApiResponse> Current()
            => await _tenantManager.Get(_db.TenantId);
    }
}
