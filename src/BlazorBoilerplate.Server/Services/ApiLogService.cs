using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Shared;
using Microsoft.Extensions.Configuration;

namespace BlazorBoilerplate.Server.Services
{
    public interface IApiLogService
    {
        Task<bool> Log(ApiLogItem apiLogItem);
        Task<IEnumerable<ApiLogItem>> Get();
    }
    public class ApiLogService : IApiLogService
    {
        private readonly ApplicationDbContext _db;
        private IConfiguration _configuration { get; set; }

        public ApiLogService(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<bool> Log(ApiLogItem apiLogItem)
        {
            try
            {
                // TODO Fix Entity Framework
                _db.ApiLogs.Add(apiLogItem);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return true;
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
