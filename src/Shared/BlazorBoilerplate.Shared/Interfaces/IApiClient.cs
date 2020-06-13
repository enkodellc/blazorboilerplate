using BlazorBoilerplate.Shared.Dto.Db;
using Breeze.Sharp;
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

        Task<QueryResult<Todo>> GetToDos();
    }
}
