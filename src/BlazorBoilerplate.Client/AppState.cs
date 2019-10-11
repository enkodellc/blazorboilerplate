using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Client.States;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Client
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

            if (apiResponse.StatusCode == 200)
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
            if (UserProfile ==  null)
            {
                UserProfile = await GetUserProfile();
            }
            if (UserProfile != null)
            {
                UserProfile.LastPageVisited = uri;
                await UpdateUserProfile();
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
