using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ApiLogController : ControllerBase
    {
        ILogger<ApiLogController> _logger;
        private readonly ApiLogService _apiLogService;

        public ApiLogController(ApiLogService apiLogService, ILogger<ApiLogController> logger)
        {
            _logger = logger;
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
