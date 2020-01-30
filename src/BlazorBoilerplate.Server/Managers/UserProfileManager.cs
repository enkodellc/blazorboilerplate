using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Stores;
using IdentityModel;
using Microsoft.AspNetCore.Http;

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

        public string GetLastPageVisited(string userName)
        => _userProfileStore.GetLastPageVisited(userName);

        public async Task<ApiResponse> Get()
        {
            var userId = new Guid(_httpContextAccessor.HttpContext.User.FindFirst(JwtClaimTypes.Subject).Value);

            var userProfile = _userProfileStore.Get(userId);

            return new ApiResponse(200, "Retrieved User Profile", userProfile);
        }

        public async Task<ApiResponse> Upsert(UserProfileDto userProfileDto)
        {
            try
            {
                await _userProfileStore.Upsert(userProfileDto);

                return new ApiResponse(200, "Updated User Profile");
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, "Failed to Retrieve User Profile");
            }
        }
    }
}
