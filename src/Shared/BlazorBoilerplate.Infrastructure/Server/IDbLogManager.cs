using BlazorBoilerplate.Infrastructure.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.DataModels;
using System.Linq.Expressions;
using System.Threading;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IDbLogManager
    {
        Task Log(DbLog LogItem);
        Task<ApiResponse> Get(int pageSize, int page, Expression<Func<DbLog, bool>> predicate = null, CancellationToken cancellationToken = default);
    }
}
