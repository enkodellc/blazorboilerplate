using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Storage.Stores
{
    public class TenantStore : ITenantStore
    {
        private readonly IApplicationDbContext _db;

        public TenantStore(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Tenant>> GetAll() => await _db.Tenants.ToListAsync();

        public async Task<Tenant> GetById(Guid id) => await _db.Tenants.FindAsync(id);

        public async Task<Tenant> Create(Tenant tenant)
        {
            Tenant t = new Tenant
            {
                Title = tenant.Title
            };
            await _db.Tenants.AddAsync(t);
            await _db.SaveChangesAsync(CancellationToken.None);
            return t;
        }

        public async Task<Tenant> Update(Tenant tenant)
        {
            Tenant t = _db.Tenants.Find(tenant.Id);
            t.Title = tenant.Title;
            await _db.SaveChangesAsync(CancellationToken.None);
            return t;
        }

        public async Task DeleteById(Guid id)
        {
            var tenant = _db.Tenants.FirstOrDefault(t => t.Id == id);

            if (tenant == null)
                throw new InvalidDataException($"Unable to find Tenant with ID: {id}");

            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
