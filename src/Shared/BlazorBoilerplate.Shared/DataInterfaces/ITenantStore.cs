using BlazorBoilerplate.Shared.Dto.Tenant;
using Finbuckle.MultiTenant;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.DataInterfaces
{
    public interface ITenantStore
    {
        Task<List<TenantInfo>> GetAll();

        Task<TenantInfo> GetById(string id);

        Task<TenantInfo> Create(TenantDto tenantDto);

        Task<TenantInfo> Update(TenantDto tenantDto);

        Task DeleteById(string id);
    }
}