using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IUserProfileManager
    {
        Task<ApiResponse> Get();
        Task<ApiResponse> Upsert(UserProfileDto userProfile);
        string GetLastPageVisited(string userName);
    }
}