using BlazorBoilerplate.Infrastructure.Server.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityServerController : ControllerBase
    {
        private readonly IIdentityServerInteractionService _interaction;
        ILogger<IdentityServerController> _logger;

        public IdentityServerController(IIdentityServerInteractionService interaction, ILogger<IdentityServerController> logger)
        {
            _interaction = interaction;
            _logger = logger;
        }

        [HttpGet("GetError")]
        public async Task<ApiResponse> GetError([FromQuery] string errorId)
        {
            try
            {
                var message = await _interaction.GetErrorContextAsync(errorId);

                return new ApiResponse(Status200OK, null, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"IdentityServerController GetError Exception {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
    }
}
