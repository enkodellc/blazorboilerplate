using System;
using System.Threading;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class ApiLogManager : IApiLogManager
    {
        private readonly IApiLogStore _apiLogStore;
        private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
        private readonly IUserSession _userSession;

        public ApiLogManager(IConfiguration configuration, IApiLogStore apiLogStore, IApplicationDbContext db, IUserSession userSession)
        {
            _apiLogStore = apiLogStore;
            _userSession = userSession;

            // Calling Log from the API Middlware results in a disposed ApplicationDBContext. This is here to build a DB Context for logging API Calls
            // If you have a better solution please let me know.
            _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (Convert.ToBoolean(configuration["BlazorBoilerplate:UsePostgresServer"] ?? "false"))
            {
                _optionsBuilder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"));
            }
            else if (Convert.ToBoolean(configuration["BlazorBoilerplate:UseSqlServer"] ?? "false"))
            {
                _optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection")); //SQL Server Database
            }
            else
            {
                _optionsBuilder.UseSqlite($"Filename={configuration.GetConnectionString("SqlLiteConnectionFileName")}");  // Sql Lite / file database
            }
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
            {
                apiLogItem.ApplicationUserId = null;
            }

            using (var dbContext = new ApplicationDbContext(_optionsBuilder.Options, _userSession))
            {
                dbContext.ApiLogs.Add(apiLogItem);
                await dbContext.SaveChangesAsync(CancellationToken.None);
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
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }
    }
}
