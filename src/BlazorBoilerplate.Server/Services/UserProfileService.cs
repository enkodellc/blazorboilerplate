using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;

namespace BlazorBoilerplate.Server.Services
{
    public interface IUserProfileService
    {
        Task<ApiResponse> Get(Guid userId);
        Task<ApiResponse> Upsert(UserProfileDto userProfile);
    }
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _db;

        public UserProfileService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResponse> Get(Guid userId)
        {
            try
            {
                var profileQuery = from userProf in _db.UserProfiles
                              where userProf.ApplicationUser.Id == userId
                              select userProf;

                UserProfileDto userProfile = new UserProfileDto();
                if (!profileQuery.Any())
                {
                    userProfile = new UserProfileDto
                    {
                        UserId = userId
                    };
                }
                else
                {
                    UserProfile profile = profileQuery.First();
                    userProfile.Count = profile.Count;
                    userProfile.IsNavOpen = profile.IsNavOpen;
                    userProfile.LastPageVisited = profile.LastPageVisited;
                    userProfile.IsNavMinified = profile.IsNavMinified;
                }

                return new ApiResponse(200, "Retrieved User Profile", userProfile);
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new ApiResponse(400, "Failed to Retrieve User Profile");
            }
        }

        public async Task<ApiResponse> Upsert(UserProfileDto userProfile)
        {
            try
            {
                var profileQuery = from prof in _db.UserProfiles where prof.ApplicationUser.Id == userProfile.UserId select prof;

                UserProfile profile = new UserProfile();

                if (profileQuery.Any())
                {
                    profile = profileQuery.First();
                }

                    profile.Count = userProfile.Count;
                    profile.IsNavOpen = userProfile.IsNavOpen;
                    profile.LastPageVisited = userProfile.LastPageVisited;
                    profile.IsNavMinified = userProfile.IsNavMinified;
                    profile.LastUpdatedDate = DateTime.Now;

                if (profileQuery.Any())
                {
                    _db.UserProfiles.Update(profile);
                }
                else
                {
                    _db.UserProfiles.Add(profile);
                }

                await _db.SaveChangesAsync();

                return new ApiResponse(200, "Updated User Profile");
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new ApiResponse(400, "Failed to Retrieve User Profile");
            }
        }
    }
}
