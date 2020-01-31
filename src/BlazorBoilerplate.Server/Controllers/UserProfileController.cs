using System;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using Microsoft.AspNetCore.Http;
using IdentityModel;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly ILogger<UserProfileController> _logger;
        private readonly IUserProfileManager _userProfileManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProfileController(IUserProfileManager userProfileManager, ILogger<UserProfileController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userProfileManager = userProfileManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/UserProfile
        [HttpGet("Get")]
        public async Task<ApiResponse> Get() 
            => await _userProfileManager.Get();

        // POST: api/UserProfile
        [HttpPost("Upsert")]
        public async Task<ApiResponse> Upsert(UserProfileDto userProfile) 
            => ModelState.IsValid ? 
                await _userProfileManager.Upsert(userProfile) :
                new ApiResponse(400, "User Model is Invalid");
    }
}
