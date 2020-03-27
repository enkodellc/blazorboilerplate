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
                                     select userProf).SingleOrDefaultAsync();

            if (userProfile != null)
                lastPageVisited = !string.IsNullOrEmpty(userProfile.LastPageVisited) ? userProfile.LastPageVisited : lastPageVisited;

            return lastPageVisited;
        }


        public async Task<UserProfileDto> Get(Guid userId)
        {
            var profileQuery = from userProf in _applicationDbContext.UserProfiles
                               where userProf.UserId == userId
                               select userProf;

            var userProfile = new UserProfileDto
            {
                UserId = userId
            };

            var profile = await profileQuery.SingleOrDefaultAsync();

            if (profile != null)
            {
                userProfile.Count = profile.Count;
                userProfile.IsNavOpen = profile.IsNavOpen;
                userProfile.LastPageVisited = profile.LastPageVisited;
                userProfile.IsNavMinified = profile.IsNavMinified;
            }

            return userProfile;
        }

        public async Task Upsert(UserProfileDto userProfileDto)
        {
            var profileQuery = from prof in _applicationDbContext.UserProfiles
                               where prof.UserId == userProfileDto.UserId
                               select prof;

            var profile = await profileQuery.SingleOrDefaultAsync();

            bool toInsert;

            if (toInsert = profile == null)
                profile = new UserProfile();

            profile.UserId = userProfileDto.UserId;
            profile.Count = userProfileDto.Count;
            profile.IsNavOpen = userProfileDto.IsNavOpen;
            profile.LastPageVisited = userProfileDto.LastPageVisited;
            profile.IsNavMinified = userProfileDto.IsNavMinified;
            profile.LastUpdatedDate = DateTime.Now;

            if (toInsert)
                _applicationDbContext.UserProfiles.Add(profile);
            else
                _applicationDbContext.UserProfiles.Update(profile);

            await _applicationDbContext.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteAllApiLogsForUser(Guid userId)
        {
            var apiLogs = await _applicationDbContext.ApiLogs.Where(a => a.ApplicationUserId == userId).ToArrayAsync(); // This could be handled in a store, getting rid of the ugliness here.

            //TODO change implementation, because with a huge amount of items it will time out 
            _applicationDbContext.ApiLogs.RemoveRange(apiLogs);

            await _applicationDbContext.SaveChangesAsync(CancellationToken.None);
        }
    }
}
