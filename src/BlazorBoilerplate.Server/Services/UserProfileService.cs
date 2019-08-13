using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Server.Middleware.Wrappers;

namespace BlazorBoilerplate.Server.Services
{
    public interface IUserProfileService
    {
        Task<APIResponse> Get(Guid userId);
        Task<APIResponse> Upsert(UserProfile userProfile);
    }
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _db;

        public UserProfileService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<APIResponse> Get(Guid userId)
        {
            try
            {
                var profile = from userProf in _db.UserProfiles
                              where userProf.UserId == userId
                              select userProf;

                UserProfile userProfile = new UserProfile();
                if (!profile.Any())
                {
                    userProfile = new UserProfile
                    {
                        UserId = userId
                    };
                }
                else
                {
                    userProfile = profile.First();
                }

                return new APIResponse(200, "Retrieved User Profile", userProfile);
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to Retrieve User Profile");
            }
        }

        public async Task<APIResponse> Upsert(UserProfile userProfile)
        {
            try
            {
                var profiles = from prof in _db.UserProfiles where prof.UserId == userProfile.UserId select prof;

                if (profiles.Any())
                {
                    UserProfile profile = profiles.First();
                    profile.Count = userProfile.Count;
                    profile.IsNavOpen = userProfile.IsNavOpen;
                    profile.LastPageVisited = userProfile.LastPageVisited;
                    profile.IsNavMinified = userProfile.IsNavMinified;
                    profile.LastUpdatedDate = userProfile.LastUpdatedDate;
                    _db.UserProfiles.Update(profile);
                }
                else
                {
                    _db.UserProfiles.Add(userProfile);
                }
                await _db.SaveChangesAsync();

                return new APIResponse(200, "Updated User Profile");
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to Retrieve User Profile");
            }
        }
    }
}
