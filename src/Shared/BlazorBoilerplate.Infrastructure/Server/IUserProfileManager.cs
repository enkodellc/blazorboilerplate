using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Account;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IUserProfileManager
    {
        Task<ApiResponse> Get();
        Task<ApiResponse> Upsert(UserProfileDto userProfile);
        Task<string> GetLastPageVisited(string userName);
    }
}