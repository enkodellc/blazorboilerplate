using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.Dto;
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
        public async Task<APIResponse> Get()
        {
            return await _todoService.Get();
        }

        // GET: api/Todo/5  
        [HttpGet("{id}")]
        public async Task<APIResponse> Get(int id)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "Todo Model is Invalid");
            }
            return await _todoService.Get(id);
        }

        // POST: api/Todos 
        [HttpPut]
        public async Task<APIResponse> Put([FromBody] TodoDto todo)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "Todo Model is Invalid");
            }
            return await _todoService.Create(todo);
        }

        // POST: api/Todos 
        [HttpPost]
        public async Task<APIResponse> Post([FromBody] TodoDto todo)
        {
            return await _todoService.Update(todo);
        }

        // DELETE: api/Todos/5  
        [HttpDelete("{id}")]
        public async Task<APIResponse> Delete(long id)
        {
            //if (isHard)
            //{
                return await _todoService.HardDelete(id); // Delete from DB  
            //}            
            //return await _todoService.SoftDelete(id); // Set Deleted flag.  
        }
    }
}
