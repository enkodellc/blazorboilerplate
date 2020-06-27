using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class ApiLogManager : IApiLogManager
    {
        private readonly IApiLogStore _apiLogStore;
        private readonly IUserSession _userSession;
        private readonly IServiceScopeFactory _scopeFactory;

        public ApiLogManager(IApiLogStore apiLogStore, IUserSession userSession, IServiceScopeFactory scopeFactory)
        {
            _apiLogStore = apiLogStore;
            _userSession = userSession;
            _scopeFactory = scopeFactory;
        }

        public async Task Log(ApiLogItem apiLogItem)
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


            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.ApiLogs.Add(apiLogItem);

                await dbContext.SaveChangesAsync();
            }            
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