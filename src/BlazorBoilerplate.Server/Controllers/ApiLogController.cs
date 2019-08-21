using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task<ApiResponse> Get()
        {
            return await _apiLogService.Get();
        }
    }
}
