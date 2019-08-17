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
        Task<APIResponse> Get(Guid userId);
        Task<APIResponse> Upsert(UserProfileDto userProfile);
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
                var profileQuery = from userProf in _db.UserProfiles
                              where userProf.UserId == userId
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

                return new APIResponse(200, "Retrieved User Profile", userProfile);
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to Retrieve User Profile");
            }
        }

        public async Task<APIResponse> Upsert(UserProfileDto userProfile)
        {
            try
            {
                var profileQuery = from prof in _db.UserProfiles where prof.UserId == userProfile.UserId select prof;

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
