using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto.AutoML;
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
    public class AutoMlController : Controller
    {
        private readonly IStringLocalizer<Global> L;
        private readonly IAutoMlManager _autoMlManager;
        public AutoMlController(IStringLocalizer<Global> l, IAutoMlManager autoMlManager)
        {
            L = l;
            _autoMlManager = autoMlManager;
        }

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> GetAutoMlModel(GetAutoMlModelRequestDto autoMl)
            => ModelState.IsValid ?
                await _autoMlManager.GetModel(autoMl) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> StartAuto(StartAutoMLRequestDto autoMl)
            => ModelState.IsValid ?
                await _autoMlManager.Start(autoMl) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> TestAutoML(TestAutoMLRequestDto autoMl)
            => ModelState.IsValid ?
                await _autoMlManager.TestAutoML(autoMl) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);
    }
}
