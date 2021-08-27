﻿using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto.Ontology;
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
    public class OntologyController : Controller
    {
        private readonly IStringLocalizer<Global> L;
        private readonly IOntologyManager _ontologyManager;
        public OntologyController(IStringLocalizer<Global> l, IOntologyManager ontologyManager)
        {
            L = l;
            _ontologyManager = ontologyManager;
        }

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> GetTasks(GetTasksRequestDto dataset)
            => ModelState.IsValid ?
                await _ontologyManager.GetTasks(dataset) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);

    }
}
