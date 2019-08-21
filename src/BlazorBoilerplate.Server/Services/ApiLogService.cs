using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Server.Data;
using Microsoft.Extensions.Configuration;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace BlazorBoilerplate.Server.Services
{
    public interface IApiLogService
    {
        Task Log(ApiLogItem apiLogItem, Guid userId);
        Task<ApiResponse> Get();
    }
    public class ApiLogService : IApiLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
        private readonly IMapper _autoMapper;
        private UserManager<ApplicationUser> _userManager;

        public ApiLogService(IConfiguration configuration, ApplicationDbContext db, IMapper autoMapper, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _autoMapper = autoMapper;
            _userManager = userManager;

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

        public async Task Log(ApiLogItem apiLogItem, Guid userId)
        {
            using (ApplicationDbContext _dbContext = new ApplicationDbContext(_optionsBuilder.Options))
            {
                if (userId != Guid.Empty)
                {
                    apiLogItem.ApplicationUser = await _userManager.FindByIdAsync(userId.ToString());
                }
                _dbContext.ApiLogs.Add(apiLogItem);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<ApiResponse> Get()
        {
            try
            {
                return new ApiResponse(200, "Retrieved Todos", _autoMapper.ProjectTo<ApiLogItemDto>(_db.ApiLogs));
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, ex.Message);
            }
        }
    }
}
