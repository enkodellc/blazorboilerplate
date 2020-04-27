using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Sample;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface ITodoManager
    {
        Task<ApiResponse> Get();
        Task<ApiResponse> Get(long id);
        Task<ApiResponse> Create(TodoDto todo);
        Task<ApiResponse> Update(TodoDto todo);
        Task<ApiResponse> Delete(long id);
    }
}