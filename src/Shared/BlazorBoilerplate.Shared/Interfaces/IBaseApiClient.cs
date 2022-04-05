using Breeze.Sharp;
using System.Linq.Expressions;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IBaseApiClient
    {
        void AddEntity(IEntity entity);

        void RemoveEntity(IEntity entity);

        void ClearEntitiesCache();

        void CancelChanges();

        event EventHandler<EntityChangedEventArgs> EntityChanged;

        Task SaveChanges();

        Task<QueryResult<T>> GetItems<T>(string from,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            Expression<Func<T, object>> orderByDescending = null,
            int? take = null,
            int? skip = null,
            Dictionary<string, object> parameters = null);

        Task<QueryResult<T>> GetItemsByFilter<T>(
            string from,
            string orderByDefaultField,
            string filter = null,
            string orderBy = null,
            string orderByDescending = null,
            int? take = null, int? skip = null,
            Dictionary<string, object> parameters = null);
    }
}
