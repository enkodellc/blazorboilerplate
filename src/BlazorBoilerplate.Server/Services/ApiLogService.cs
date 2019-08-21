using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Server.Data;
using Microsoft.Extensions.Configuration;
using BlazorBoilerplate.Server.Models;

namespace BlazorBoilerplate.Server.Services
{
    public interface IApiLogService
    {
        Task Log(ApiLogItem apiLogItem);
        Task<IEnumerable<ApiLogItem>> Get();
    }
    public class ApiLogService : IApiLogService
    {
        ApplicationDbContext _db;
        DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

        public ApiLogService(IConfiguration configuration, ApplicationDbContext db)
        {
            _db = db;

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
            using (ApplicationDbContext _dbContext = new ApplicationDbContext(_optionsBuilder.Options))
            {
                _dbContext.ApiLogs.Add(apiLogItem);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ApiLogItem>> Get()
        {
            var items = from i in _db.ApiLogs
                        orderby i.Id descending
                        select i;

            return await items.ToListAsync();
        }
    }
}
