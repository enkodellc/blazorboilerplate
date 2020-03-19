using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Storage.Stores
{
    public class UserProfileStore : IUserProfileStore
    {
        private readonly IApplicationDbContext _applicationDbContext;

        public UserProfileStore(IApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<string> GetLastPageVisited(string userName)
        {
            string lastPageVisited = "/dashboard";
            var userProfile = await (from userProf in _applicationDbContext.UserProfiles
                                     join user in _applicationDbContext.Users on userProf.UserId equals user.Id
                                     where user.UserName == userName
                                     select userProf).FirstOrDefaultAsync();

            if (userProfile != null)
            {
                lastPageVisited = !String.IsNullOrEmpty(userProfile.LastPageVisited) ? userProfile.LastPageVisited : lastPageVisited;
            }

            return lastPageVisited;
        }


        public async Task<UserProfileDto> Get(Guid userId)
        {
            var profileQuery = from userProf in _applicationDbContext.UserProfiles
                where userProf.UserId == userId
                select userProf;

            var userProfile = new UserProfileDto();
            if (!profileQuery.Any())
            {
                userProfile = new UserProfileDto
                {
                    UserId = userId
                };
            }
            else
            {
                var profile = profileQuery.First();
                userProfile.Count = profile.Count;
                userProfile.IsNavOpen = profile.IsNavOpen;
                userProfile.LastPageVisited = profile.LastPageVisited;
                userProfile.IsNavMinified = profile.IsNavMinified;
                userProfile.UserId = userId;
            }

            return userProfile;
        }

        public async Task Upsert(UserProfileDto userProfileDto)
        {
            var profileQuery = from prof in _applicationDbContext.UserProfiles where prof.UserId == userProfileDto.UserId select prof;
            if (profileQuery.Any())
            {
                var profile = profileQuery.First();

                profile.Count = userProfileDto.Count;
                profile.IsNavOpen = userProfileDto.IsNavOpen;
                profile.LastPageVisited = userProfileDto.LastPageVisited;
                profile.IsNavMinified = userProfileDto.IsNavMinified;
                profile.LastUpdatedDate = DateTime.Now;
                _applicationDbContext.UserProfiles.Update(profile);
            }
            else
            {
                var profile = new UserProfile
                {
                    UserId = userProfileDto.UserId,
                    Count = userProfileDto.Count,
                    IsNavOpen = userProfileDto.IsNavOpen,
                    LastPageVisited = userProfileDto.LastPageVisited,
                    IsNavMinified = userProfileDto.IsNavMinified,
                    LastUpdatedDate = DateTime.Now
                };
                _applicationDbContext.UserProfiles.Add(profile);
            }

            await _applicationDbContext.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteAllApiLogsForUser(Guid userId)
        {
            var apiLogs = _applicationDbContext.ApiLogs.Where(a => a.ApplicationUserId == userId); // This could be handled in a store, getting rid of the ugliness here.
            foreach (var apiLog in apiLogs)
            {
                _applicationDbContext.ApiLogs.Remove(apiLog);
            }
            _applicationDbContext.SaveChanges();
        }
    }
}
