using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Client.Services.Contracts
{
    /// <summary>
    /// Access to User Profile information
    /// </summary>
    public interface IUserProfileApi
    {
        Task<ApiResponseDto> Upsert(UserProfileDto userProfile);
        Task<ApiResponseDto> Get();
    }
}
