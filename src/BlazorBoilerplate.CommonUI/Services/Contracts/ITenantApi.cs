using BlazorBoilerplate.Shared.DataModels;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.Services.Contracts
{
    public interface ITenantApi
    {
        Tenant Tenant { get; }

        Task<Tenant> GetUserTenant();
    }
}