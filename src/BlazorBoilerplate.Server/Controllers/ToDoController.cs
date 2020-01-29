using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.Dto.Sample;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly ILogger<ToDoController> _logger;
        private readonly ITodoManager _todoManager;

        public ToDoController(ITodoManager todoManager, ILogger<ToDoController> logger)
        {
            _logger = logger;
            _todoManager = todoManager;
        }
                
        // GET: api/Todo
        [HttpGet]
        [AllowAnonymous]
        public async Task<ApiResponse> Get()
        {
            return await _todoManager.Get();
        }
                
        // GET: api/Todo/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ApiResponse> Get(int id)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "Todo Model is Invalid");
            }
            return await _todoManager.Get(id);
        }
                
        // POST: api/Todo
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResponse> Post([FromBody] TodoDto todo)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "Todo Model is Invalid");
            }
            return await _todoManager.Create(todo);
        }
                
        // Put: api/Todo
        [HttpPut]
        [AllowAnonymous]
        public async Task<ApiResponse> Put([FromBody] TodoDto todo)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "Todo Model is Invalid");
            }
            return await _todoManager.Update(todo);
        }                
        
        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> Delete(long id)
        {
            return await _todoManager.Delete(id); // Delete from DB
        }
    }
}
