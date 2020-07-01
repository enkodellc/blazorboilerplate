using System;
using System.Threading.Tasks;

using static Microsoft.AspNetCore.Http.StatusCodes;

using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Account;
using Newtonsoft.Json;

namespace BlazorBoilerplate.CommonUI
{
    public class AppState
    {
        public event Action OnChange;
        private readonly IUserProfileApi _userProfileApi;

        public UserProfileDto UserProfile { get; set; }

        public AppState(IUserProfileApi userProfileApi)
        {
            _userProfileApi = userProfileApi;
        }

        public bool IsNavOpen
        {
            get
            {
                if (UserProfile == null)
                {
                    return true;
                }
                return UserProfile.IsNavOpen;
            }
            set
            {
                UserProfile.IsNavOpen = value;
            }
        }
        public bool IsNavMinified { get; set; }

        public async Task UpdateUserProfile()
        {
            await _userProfileApi.Upsert(UserProfile);
        }

        public async Task<UserProfileDto> GetUserProfile()
        {
            if (UserProfile != null && UserProfile.UserId != Guid.Empty)
            {
                return UserProfile;
            }

            ApiResponseDto apiResponse = await _userProfileApi.Get();

            if (apiResponse.StatusCode == Status200OK)
            {
                return JsonConvert.DeserializeObject<UserProfileDto>(apiResponse.Result.ToString());
            }
            return new UserProfileDto();
        }

        public async Task UpdateUserProfileCount(int count)
        {
            UserProfile.Count = count;
            await UpdateUserProfile();
            NotifyStateChanged();
        }

        public async Task<int> GetUserProfileCount()
        {
            if (UserProfile == null)
            {
                UserProfile = await GetUserProfile();
                return UserProfile.Count;
            }

            return UserProfile.Count;
        }

        public async Task SaveLastVisitedUri(string uri)
        {
            if (UserProfile == null)
            {
                UserProfile = await GetUserProfile();
            }
            else
            {
                UserProfile.LastPageVisited = uri;
                await UpdateUserProfile();
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
