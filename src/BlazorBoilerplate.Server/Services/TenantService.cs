using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto.Tenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Services
{
    public interface ITenantService
    {
        Task<ApiResponse> GetTenants();

        Task<ApiResponse> GetTenant(int id);

        Task<ApiResponse> PutTenant(int id, TenantDto tenant);

        Task<ApiResponse> PostTenant(TenantDto tenant);

        Task<ApiResponse> DeleteTenant(int id);
    }

    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _autoMapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public TenantService(ApplicationDbContext db, IMapper autoMapper, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _autoMapper = autoMapper;
            _userManager = userManager;
        }

        public async Task<ApiResponse> GetTenants() => new ApiResponse(200, "Retrieved Api Log", _autoMapper.ProjectTo<TenantDto>(_db.Tenants));

        public async Task<ApiResponse> GetTenant(int id)
        {
            TenantDto tenantDto = new TenantDto();
            _autoMapper.Map(await _db.Tenants.FindAsync(id), tenantDto);
            tenantDto.OwnerName = (await _userManager.FindByIdAsync(tenantDto.OwnerUserId.ToString())).FullName;
            return new ApiResponse(200, "Retrieved Api Log", tenantDto);
        }

        public async Task<ApiResponse> PutTenant(int id, TenantDto tenant)
        {
            _db.Tenants.Find(id);
            Tenant t = _db.Tenants.Find(id);
            _autoMapper.Map(tenant, t);
            try
            {
                await _db.SaveChangesAsync();
                return new ApiResponse(200, "Tenant Updated", _autoMapper.Map(t, new TenantDto()));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(id))
                {
                    return new ApiResponse(404, "Tenant Not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<ApiResponse> PostTenant(TenantDto tenant)
        {
            Tenant t = new Tenant();
            await _db.Tenants.AddAsync(_autoMapper.Map(tenant, t));
            await _db.SaveChangesAsync();

            return new ApiResponse(200, "Tenant Created", _autoMapper.Map(t, new TenantDto()));
        }

        public async Task<ApiResponse> DeleteTenant(int id)
        {
            var tenant = await _db.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return new ApiResponse(404, "Tenant Not found");
            }

            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();

            return new ApiResponse(200, "Tenant Removed", _autoMapper.Map(tenant, new TenantDto()));
        }

        private bool TenantExists(int id)
        {
            return _db.Tenants.Any(e => e.Id == id);
        }
    }
}