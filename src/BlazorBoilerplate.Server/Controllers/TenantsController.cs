using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Authorization;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.Dto.Tenant;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]//TODO Roles of managing tenants
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ApiResponse _invalidModel;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
            _invalidModel = new ApiResponse(400, "User Model is Invalid");
        }

        // GET: api/Tenants
        [HttpGet]
        public async Task<ApiResponse> GetTenants() => await _tenantService.GetTenants();

        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetTenant(int id) => await _tenantService.GetTenant(id);

        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        public async Task<ApiResponse> PutTenant(int id, [FromBody] TenantDto tenant) => ModelState.IsValid ? await _tenantService.PutTenant(id, tenant) : _invalidModel;

        // POST: api/Tenants
        [HttpPost]
        public async Task<ApiResponse> PostTenant([FromBody] TenantDto tenant) => ModelState.IsValid ? await _tenantService.PostTenant(tenant) : _invalidModel;

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteTenant(int id) => await _tenantService.DeleteTenant(id);
    }
}