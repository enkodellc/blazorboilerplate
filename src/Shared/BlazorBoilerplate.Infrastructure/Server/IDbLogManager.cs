using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server.Models;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IDbLogManager
    {
        Task Log(DbLog LogItem);
        Task<ApiResponse> Get(int pageSize, int page, Expression<Func<DbLog, bool>> predicate = null, CancellationToken cancellationToken = default);
    }
}
