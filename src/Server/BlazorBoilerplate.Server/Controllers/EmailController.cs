using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Dto.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailManager _emailManager;
        private readonly IStringLocalizer<Global> L;

        public EmailController(IEmailManager emailManager, IStringLocalizer<Global> l)
        {
            _emailManager = emailManager;
            L = l;
        }

        [HttpPost("Send")]
        [ProducesResponseType((int)Status200OK)]
        [ProducesResponseType((int)Status400BadRequest)]
        public async Task<ApiResponse> Send(EmailDto parameters)
            => ModelState.IsValid ?
                await _emailManager.Send(parameters) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);

        [HttpGet("Receive")]
        [Authorize]
        public async Task<ApiResponse> Receive()
            => await _emailManager.ReceiveMailImapAsync();
    }
}
