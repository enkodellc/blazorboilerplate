using System;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IApiLogManager
    {
        Task Log(ApiLogItem apiLogItem, IApplicationDbContext db);
        Task<ApiResponse> Get();
        Task<ApiResponse> GetByApplicationUserId(Guid applicationUserId);
    }
}