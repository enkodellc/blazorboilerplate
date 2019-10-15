using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly ILogger<ToDoController> _logger;
        private readonly ITodoService _todoService;

        public ToDoController(ITodoService todoService, ILogger<ToDoController> logger)
        {
            _logger = logger;
            _todoService = todoService;
        }
                
        // GET: api/Todo
        [HttpGet]
        [AllowAnonymous]
        public async Task<ApiResponse> Get()
        {
            return await _todoService.Get();
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
            return await _todoService.Get(id);
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
            return await _todoService.Create(todo);
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
            return await _todoService.Update(todo);
        }                
        
        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> Delete(long id)
        {
            return await _todoService.Delete(id); // Delete from DB
        }
    }
}
