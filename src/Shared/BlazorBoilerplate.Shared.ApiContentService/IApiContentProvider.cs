using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.DataInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.ApiContentService
{
    public interface IApiContentProvider
    {
        Task<bool> Upsert(WikiPage property);
        Task<int> SaveChangesAsync();
    }
}
