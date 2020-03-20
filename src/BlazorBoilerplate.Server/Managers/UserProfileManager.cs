using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class UserProfileManager : IUserProfileManager
    {
        private readonly IUserProfileStore _userProfileStore;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProfileManager(IUserProfileStore userProfileStore, IHttpContextAccessor httpContextAccessor)
        {
            _userProfileStore = userProfileStore;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetLastPageVisited(string userName)
        => await _userProfileStore.GetLastPageVisited(userName);

        public async Task<ApiResponse> Get()
        {
            var userId = new Guid(_httpContextAccessor.HttpContext.User.FindFirst(JwtClaimTypes.Subject).Value);

            var userProfile = await _userProfileStore.Get(userId);

            return new ApiResponse(Status200OK, "Retrieved User Profile", userProfile);
        }

        public async Task<ApiResponse> Upsert(UserProfileDto userProfileDto)
        {
            try
            {
                await _userProfileStore.Upsert(userProfileDto);

                return new ApiResponse(Status200OK, "Updated User Profile");
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, "Failed to Retrieve User Profile");
            }
        }
    }
}
