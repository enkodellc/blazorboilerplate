using System.Threading.Tasks;
using BlazorBoilerplate.Shared;

namespace BlazorBoilerplate.Client.Services.Contracts
{
    /// <summary>
    /// Access to User Profile information
    /// </summary>
    public interface IUserProfileApi
    {
        Task<ClientApiResponse> Upsert(UserProfile userProfile);
        Task<ClientApiResponse> Get();
    }
}
