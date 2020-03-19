using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.Dto.Tenant;

namespace BlazorBoilerplate.Storage.Stores
{
    public class TenantStore : ITenantStore
    {
        private readonly IApplicationDbContext _db;
        private readonly IMapper _autoMapper;

        public TenantStore(IApplicationDbContext db, IMapper autoMapper)
        {
            _db = db;
            _autoMapper = autoMapper;
        }

        public List<TenantDto> GetAll()
        {
            throw new System.NotImplementedException();
            //return _autoMapper.ProjectTo<TenantDto>(_db.Tenants).ToList();
        }

        public TenantDto GetById(long id)
        {
            throw new System.NotImplementedException();
            //var tenant = _db.Tenants.FirstOrDefault(t => t.Id == id);

            //if (tenant == null)
            //    throw new InvalidDataException($"Unable to find Tenant with ID: {id}");

            //return _autoMapper.Map<TenantDto>(tenant);
        }

        public async Task<Tenant> Create(TenantDto tenantDto)
        {
            throw new System.NotImplementedException();
            //var tenant = _autoMapper.Map<TenantDto, Tenant>(tenantDto);
            //await _db.Tenants.AddAsync(tenant);
            //await _db.SaveChangesAsync(CancellationToken.None);
            //return tenant;
        }

        public async Task<Tenant> Update(TenantDto tenantDto)
        {
            throw new System.NotImplementedException();
            //var tenant = _db.Tenants.FirstOrDefault(t => t.Id == tenantDto.Id);
            //if (tenant == null)
            //    throw new InvalidDataException($"Unable to find Tenant with ID: {tenantDto.Id}");

            //tenant = _autoMapper.Map(tenantDto, tenant);
            //_db.Tenants.Update(tenant);
            //await _db.SaveChangesAsync(CancellationToken.None);

            //return tenant;
        }

        public async Task DeleteById(long id)
        {
            throw new System.NotImplementedException();
            //var tenant = _db.Tenants.FirstOrDefault(t => t.Id == id);

            //if (tenant == null)
            //    throw new InvalidDataException($"Unable to find Tenant with ID: {id}");

            //_db.Tenants.Remove(tenant);
            //await _db.SaveChangesAsync(CancellationToken.None);
        }

        Task<Tenant> ITenantStore.Create(TenantDto tenantDto)
        {
            throw new System.NotImplementedException();
        }

        Task<Tenant> ITenantStore.Update(TenantDto tenantDto)
        {
            throw new System.NotImplementedException();
        }
    }
}
