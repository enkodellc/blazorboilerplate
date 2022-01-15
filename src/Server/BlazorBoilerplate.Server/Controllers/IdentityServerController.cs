using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Server.Aop;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
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

        public IdentityServerController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [HttpGet("GetError")]
        public async Task<ApiResponse> GetError([FromQuery] string errorId)
        {
            var message = await _interaction.GetErrorContextAsync(errorId);

            return new ApiResponse(Status200OK, null, message);
        }
    }
}
