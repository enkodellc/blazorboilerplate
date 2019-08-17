using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Server.Models;
using Microsoft.Extensions.Configuration;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ApiLogController : ControllerBase
    {
        private readonly ApiLogService _apiLogService;

        public ApiLogController(ApiLogService apiLogService)
        {
            _apiLogService = apiLogService;
        }

        // GET: api/ApiLogs
        [HttpGet]
        public async Task<IEnumerable<ApiLogItem>> Get()
        {
            var apiLogItems = await _apiLogService.Get();
            return apiLogItems;
        }
    }
}
