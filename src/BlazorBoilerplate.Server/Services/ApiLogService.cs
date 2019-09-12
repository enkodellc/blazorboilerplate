using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Services
{
    public interface IApiLogService
    {
        Task Log(ApiLogItem apiLogItem);
        Task<ApiResponse> Get();
        Task<ApiResponse> GetByApplictionUserId(Guid applicationUserId);
    }

    public class ApiLogService : IApiLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
        private readonly IMapper _autoMapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserSession _userSession;

        public ApiLogService(IConfiguration configuration, ApplicationDbContext db, IMapper autoMapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IUserSession userSession)
        {
            _db = db;
            _autoMapper = autoMapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userSession = userSession;

            // Calling Log from the API Middlware results in a disposed ApplicationDBContext. This is here to build a DB Context for logging API Calls
            // If you have a better solution please let me know.
            _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            if (Convert.ToBoolean(configuration["BlazorBoilerplate:UseSqlServer"] ?? "false"))
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
            var currentUser = _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            UserSession userSession = new UserSession();
            if (currentUser != null)
            {
                userSession = new UserSession(currentUser.Result);
            }

            using (ApplicationDbContext _dbContext = new ApplicationDbContext(_optionsBuilder.Options, userSession))
            {
                _dbContext.ApiLogs.Add(apiLogItem);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<ApiResponse> Get()
        {
            return new ApiResponse(200, "Retrieved Api Log", _autoMapper.ProjectTo<ApiLogItemDto>(_db.ApiLogs));
        }

        public async Task<ApiResponse> GetByApplictionUserId(Guid applicationUserId)
        {
            try
            {
                return new ApiResponse(200, "Retrieved Api Log", _autoMapper.ProjectTo<ApiLogItemDto>(_db.ApiLogs.Where(a => a.ApplicationUserId == applicationUserId)));
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, ex.Message);
            }
        }
    }
}
