using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Server.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using System.Linq.Expressions;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy=Policies.IsAdmin)]
    public class DbLogController : ControllerBase
    {
        private readonly IDbLogManager _dbLogManager;
        private readonly ILogger<DbLogController> _logger;

        public DbLogController(IDbLogManager dbLogManager, ILogger<DbLogController> logger)
        {
            _dbLogManager = dbLogManager;
            _logger = logger;
        }

        // GET: api/DbLog
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetLogs([FromQuery]int pageSize, [FromQuery] int page)
        {
            // TODO: Implement an api-safe client selector // filtering
            Expression<Func<DbLog, bool>> predicate = _=>  true;
                //placeholder for selector


            return await _dbLogManager.GetAsync(pageSize, page, predicate).ConfigureAwait(false);
        }

        [HttpGet("delta")]
        public async Task<ActionResult<ApiResponse>> GetLogDelta([FromQuery] int deltaIndex)
        {
            // TODO: Implement an api-safe client selector // filtering
            Expression<Func<DbLog, bool>> predicate = _ => true;
            //placeholder for selector

            return await _dbLogManager.GetDeltaMetaAsync(
                deltaIndex: deltaIndex,
                cancellationToken: default
                ).ConfigureAwait(false);


        }

    }
}
