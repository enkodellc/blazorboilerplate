using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Shared;

namespace BlazorBoilerplate.Server.Services
{
    public class ApiLogService
    {
        private readonly ApplicationDbContext _db;
        public ApiLogService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Log(ApiLogItem apiLogItem)
        {
            _db.ApiLogs.Add(apiLogItem);
            await _db.SaveChangesAsync();
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
