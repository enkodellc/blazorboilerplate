using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.Dto.Tenant;
using Finbuckle.MultiTenant;

namespace BlazorBoilerplate.Storage.Stores
{
    public class TenantStore : ITenantStore
    {
        private readonly TenantStoreDbContext _db;

        public TenantStore(TenantStoreDbContext db)
        {
            _db = db;
        }

        public List<TenantInfo> GetAll()
        {
            return _db.TenantInfo.ToList();
        }

        public TenantInfo GetById(string id)
        {
            var tenant = _db.TenantInfo.FirstOrDefault(t => t.Id == id);

            if (tenant == null)
                throw new InvalidDataException($"Unable to find Tenant with ID: {id}");

            return tenant;
        }

        public async Task<TenantInfo> Create(TenantDto tenantDto)
        {
            var tenant = new TenantInfo(tenantDto.Id, tenantDto.Identifier, tenantDto.Name, tenantDto.ConnectionString, tenantDto.Items);
            await _db.TenantInfo.AddAsync(tenant);
            await _db.SaveChangesAsync(CancellationToken.None);
            return tenant;
        }

        public async Task<TenantInfo> Update(TenantDto tenantDto)
        {
            var tenant = _db.TenantInfo.FirstOrDefault(t => t.Id == tenantDto.Id);
            if (tenant == null)
                throw new InvalidDataException($"Unable to find Tenant with ID: {tenantDto.Id}");

            tenant = new TenantInfo(tenantDto.Id, tenantDto.Identifier, tenantDto.Name, tenantDto.ConnectionString, tenantDto.Items);
            _db.TenantInfo.Update(tenant);
            await _db.SaveChangesAsync(CancellationToken.None);

            return tenant;
        }

        public async Task DeleteById(string id)
        {
            var tenant = _db.TenantInfo.FirstOrDefault(t => t.Id == id);

            if (tenant == null)
                throw new InvalidDataException($"Unable to find Tenant with ID: {id}");

            _db.TenantInfo.Remove(tenant);
            await _db.SaveChangesAsync(CancellationToken.None);
        }
    }
}