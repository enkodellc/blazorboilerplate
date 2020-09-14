using Breeze.Sharp;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IBaseApiClient
    {
        void AddEntity(IEntity entity);

        void RemoveEntity(IEntity entity);

        void ClearEntitiesCache();

        void CancelChanges();

        Task SaveChanges();
    }
}
