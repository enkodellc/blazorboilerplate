using System;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Client.Services.Contracts;
using Newtonsoft.Json;

namespace BlazorBoilerplate.Client
{
    public class AppState
    {
        public event Action OnChange;
        private readonly IUserProfileApi _userProfileApi;
        public UserProfile UserProfile { get; private set; }

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
            UserProfile.LastUpdatedDate = DateTime.Now;
            await _userProfileApi.Upsert(UserProfile);
        }

        public async Task<UserProfile> GetUserProfile()
        {
            if (UserProfile != null)
            {
                return UserProfile;
            }

            ClientApiResponse apiResponse = await _userProfileApi.Get();

            if (apiResponse.StatusCode == 200)
            {
                return JsonConvert.DeserializeObject<UserProfile>(apiResponse.Result.ToString());
            }
            return new UserProfile();
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

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
