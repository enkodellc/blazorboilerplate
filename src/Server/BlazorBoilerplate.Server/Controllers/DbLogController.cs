using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [OpenApiIgnore]
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

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetLogs([FromQuery]int pageSize, [FromQuery] int page)
        {
            // TODO: Implement an api-safe client selector // filtering
            Expression<Func<DbLog, bool>> predicate = _=>  true;
                //placeholder for selector


            return await _dbLogManager.Get(pageSize, page, predicate);
        }

    }
}
