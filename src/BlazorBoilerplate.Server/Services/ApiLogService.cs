using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace BlazorBoilerplate.Server.Services
{
    public interface IApiLogService
    {
        Task Log(ApiLogItem apiLogItem);
        Task<ApiResponse> Get();
    }
    public class ApiLogService : IApiLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
        private readonly IMapper _autoMapper;

        public ApiLogService(IConfiguration configuration, ApplicationDbContext db, IMapper autoMapper)
        {
            _db = db;
            _autoMapper = autoMapper;

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
            try
            {
                using (ApplicationDbContext _dbContext = new ApplicationDbContext(_optionsBuilder.Options))
                {
                    _dbContext.ApiLogs.Add(apiLogItem);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                //TODO Error Fix "Violation of PRIMARY KEY constraint 'PK_AspNetUsers'. Cannot insert duplicate key in object 'dbo.AspNetUsers'. The statement has been terminated."
                string test = ex.Message + " " + ex.InnerException;
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
