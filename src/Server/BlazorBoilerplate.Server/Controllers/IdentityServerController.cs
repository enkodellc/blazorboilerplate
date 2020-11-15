using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Server.Aop;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [ApiResponseException]
    [OpenApiIgnore]
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
            var message = await _interaction.GetErrorContextAsync(errorId);

            return new ApiResponse(Status200OK, null, message);
        }
    }
}
