using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto.Tenant;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ITenantManager _tenantManager;

        public TenantController(ITenantManager tenantManager)
        {
            _tenantManager = tenantManager;
        }

        // GET: api/Tenant
        [HttpGet("All")]
        public async Task<ApiResponse> GetAll()
            => await _tenantManager.Get();

        // GET: api/Tenant
        [HttpGet]
        public async Task<ApiResponse> Get()
            => await _tenantManager.Get(HttpContext.GetMultiTenantContext().TenantInfo.Id);

        // GET: api/Tenant/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> Get(string id)
            => ModelState.IsValid ?
                await _tenantManager.Get(id) :
                new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // POST: api/Tenant
        [HttpPost]
        public async Task<ApiResponse> Post([FromBody] TenantDto tenant)
            => ModelState.IsValid ?
            await _tenantManager.Create(tenant) :
            new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // PUT: api/Tenant/5
        [HttpPut("{id}")]
        public async Task<ApiResponse> Put([FromBody] TenantDto tenant)
            => ModelState.IsValid ? await _tenantManager.Update(tenant)
            : new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // DELETE: api/Tenant/5
        [HttpDelete("{id}")]
        public async Task<ApiResponse> Delete(string id)
            => await _tenantManager.Delete(id);
    }
}