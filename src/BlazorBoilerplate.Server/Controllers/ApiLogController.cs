using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Managers;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiLogController : ControllerBase
    {
        private readonly IApiLogManager _apiLogManager;

        public ApiLogController(IApiLogManager apiLogManager)
        {
            _apiLogManager = apiLogManager;
        }

        // GET: api/ApiLog
        [HttpGet]
        [AllowAnonymous]
        public async Task<ApiResponse> Get()
        =>  await _apiLogManager.Get();

        // GET: api/ApiLog/ApplicationUserId
        [HttpGet("[action]")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> GetByApplicationUserId(string userId)
        =>  await _apiLogManager.GetByApplicationUserId(new Guid(userId));
    }
}
