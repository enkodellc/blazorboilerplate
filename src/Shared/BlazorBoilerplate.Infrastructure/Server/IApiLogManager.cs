using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using System;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IApiLogManager
    {
        Task Log(ApiLogItem apiLogItem, IApplicationDbContext db);
        Task<ApiResponse> Get();
        Task<ApiResponse> GetByApplicationUserId(Guid applicationUserId);
    }
}