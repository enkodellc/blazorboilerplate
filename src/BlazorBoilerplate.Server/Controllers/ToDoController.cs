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
        private readonly ITodoManager _todoManager;

        public ToDoController(ITodoManager todoManager)
        {
            _todoManager = todoManager;
        }
                
        // GET: api/Todo
        [HttpGet]
        [AllowAnonymous]
        public async Task<ApiResponse> Get() 
            => await _todoManager.Get();

        // GET: api/Todo/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ApiResponse> Get(int id) 
            => ModelState.IsValid ? 
                await _todoManager.Get(id) :
                new ApiResponse(400, "Todo Model is Invalid");

        // POST: api/Todo
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResponse> Post([FromBody] TodoDto todo) 
            => ModelState.IsValid ? 
                await _todoManager.Create(todo) :
                new ApiResponse(400, "Todo Model is Invalid");

        // Put: api/Todo
        [HttpPut]
        [AllowAnonymous]
        public async Task<ApiResponse> Put([FromBody] TodoDto todo) 
            => ModelState.IsValid ? 
                await _todoManager.Update(todo) :
                new ApiResponse(400, "Todo Model is Invalid");

        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        [Authorize(Permissions.Todo.Delete)]
        public async Task<ApiResponse> Delete(long id) 
            => await _todoManager.Delete(id);
    }
}
