using System;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Dto.Account;

namespace BlazorBoilerplate.Shared.DataModels
{
    public interface IUserProfileStore
    {
        string GetLastPageVisited(string username);

        UserProfileDto Get(Guid userId);

        Task Upsert(UserProfileDto userProfileDto);

        Task DeleteAllApiLogsForUser(Guid userId);
    }
}