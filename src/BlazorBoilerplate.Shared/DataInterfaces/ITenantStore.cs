using BlazorBoilerplate.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.DataInterfaces
{
    public interface ITenantStore
    {
        Task<List<Tenant>> GetAll();

        Task<Tenant> GetById(Guid id);

        Task<Tenant> Create(Tenant tenant);

        Task<Tenant> Update(Tenant tenant);

        Task DeleteById(Guid id);
    }
}
