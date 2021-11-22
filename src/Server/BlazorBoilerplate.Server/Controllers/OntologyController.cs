using BlazorBoilerplate.Infrastructure.Server;
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
        public async Task<ApiResponse> GetCompatibleAutoMlSolutions(GetCompatibleAutoMlSolutionsRequestDto request)
            => ModelState.IsValid ?
                await _ontologyManager.GetCompatibleAutoMlSolutions(request) :
                new ApiResponse(Status400BadRequest, L["InvalidData"]);

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> GetSupportedMlLibraries(GetSupportedMlLibrariesRequestDto task)
            => ModelState.IsValid ?
            await _ontologyManager.GetSupportedMlLibraries(task) :
            new ApiResponse(Status400BadRequest, L["InvalidData"]);

        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ApiResponse> GetDatasetCompatibleTasks(GetDatasetCompatibleTasksRequestDto datasetName)
            => ModelState.IsValid ?
            await _ontologyManager.GetDatasetCompatibleTasks(datasetName) :
            new ApiResponse(Status400BadRequest, L["InvalidData"]);
    }
}
