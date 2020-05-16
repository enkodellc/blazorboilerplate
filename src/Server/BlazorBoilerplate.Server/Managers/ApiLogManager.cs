using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class ApiLogManager : IApiLogManager
    {
        private readonly IApiLogStore _apiLogStore;
        private readonly IUserSession _userSession;

        public ApiLogManager(IConfiguration configuration, IApiLogStore apiLogStore, IUserSession userSession)
        {
            _apiLogStore = apiLogStore;
            _userSession = userSession;
        }

        public async Task Log(ApiLogItem apiLogItem, IApplicationDbContext db)
        {
            if (apiLogItem.ApplicationUserId != Guid.Empty)
            {
                //TODO populate _userSession??

                //var currentUser = _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                //UserSession userSession = new UserSession();
                //if (currentUser != null)
                //{
                //    userSession = new UserSession(currentUser.Result);
                //}
            }
            else
                apiLogItem.ApplicationUserId = null;

            db.ApiLogs.Add(apiLogItem);
            await db.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<ApiResponse> Get()
        {
            return new ApiResponse(Status200OK, "Retrieved Api Log", await _apiLogStore.Get());
        }

        public async Task<ApiResponse> GetByApplicationUserId(Guid applicationUserId)
        {
            try
            {
                return new ApiResponse(Status200OK, "Retrieved Api Log", await _apiLogStore.GetByUserId(applicationUserId));
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.GetBaseException().Message);
            }
        }
    }
}