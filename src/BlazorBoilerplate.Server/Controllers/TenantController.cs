using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto.Tenant;
using Microsoft.AspNetCore.Authorization;
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

        // GET: api/Tenants
        [HttpGet]
        public async Task<ApiResponse> Get()
            => await _tenantManager.Get();

        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> Get(int id)
            => ModelState.IsValid ?
                await _tenantManager.Get(id) :
                new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // POST: api/Tenants
        [HttpPost]
        public async Task<ApiResponse> Post([FromBody] TenantDto tenant)
            => ModelState.IsValid ?
            await _tenantManager.Create(tenant) :
            new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        public async Task<ApiResponse> Put([FromBody] TenantDto tenant)
            => ModelState.IsValid ? await _tenantManager.Update(tenant)
            : new ApiResponse(Status400BadRequest, "Tenant Model is Invalid");

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        public async Task<ApiResponse> Delete(int id)
            => await _tenantManager.Delete(id);
    }
}
