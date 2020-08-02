using BlazorBoilerplate.Shared.Dto.Db;
using Breeze.Sharp;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IApiClient
    {
        EntityManager EntityManager { get; }

        void AddEntity(IEntity entity);

        void CancelChanges();

        Task SaveChanges();

        Task<UserProfile> GetUserProfile();

        Task<QueryResult<TenantSetting>> GetTenantSettings();

        Task<QueryResult<DbLog>> GetLogs(Expression<Func<DbLog, bool>> predicate = null, int? take = null, int? skip = null);

        Task<QueryResult<ApiLogItem>> GetApiLogs(Expression<Func<ApiLogItem, bool>> predicate = null, int? take = null, int? skip = null);

        Task<QueryResult<Todo>> GetToDos();
    }
}
