using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto.Session;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [ApiResponseException]
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class SessionController : Controller
    {
        private readonly IStringLocalizer<Global> L;
        private readonly ISessionManager _sessionManager;
        public SessionController(IStringLocalizer<Global> l, ISessionManager sessionManager)
        {
            L = l;
            _sessionManager = sessionManager;
        }

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> GetSession(GetSessionRequestDto session)
            => ModelState.IsValid ?
                await _sessionManager.GetSession(session) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> GetSessions(GetSessionsRequestDto sessions)
            => ModelState.IsValid ?
                await _sessionManager.GetSessions(sessions) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);


    }
}
