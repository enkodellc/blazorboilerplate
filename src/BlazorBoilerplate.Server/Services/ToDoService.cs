using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;

namespace BlazorBoilerplate.Server.Services
{
    public interface ITodoService
    {
        Task<APIResponse> Get();
        Task<APIResponse> Get(long id);
        Task<APIResponse> Create(TodoDto todo);
        Task<APIResponse> Update(TodoDto todo);
        Task<APIResponse> SoftDelete(long id);
        Task<APIResponse> HardDelete(long id);
    }
    public class ToDoService : ITodoService
    {
        private readonly ApplicationDbContext _db;

        public ToDoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<APIResponse> Get()
        {
            try
            {
                var toDos = from td in _db.Todos select td;
                return new APIResponse(200, "Retrieved Todos", toDos);
            }
            catch(Exception ex)
            {
                return new APIResponse(400, ex.Message);
            }
        }

        public async Task<APIResponse> Get(long id)
        {
            try
            {
                var todoResults = from td in _db.Todos where td.Id == id select td;
                return new APIResponse(200, "Retrieved User Profile", todoResults.First());
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to Retrieve User Profile");
            }
        }

        public async Task<APIResponse> Create(TodoDto todo)
        {
            try
            {
                Todo newTodo = new Todo()
                {
                    Title = todo.Title,
                    IsCompleted = todo.IsCompleted
                };
                await _db.Todos.AddAsync(newTodo);
                await _db.SaveChangesAsync();

                return new APIResponse(200, "Updated Todo");
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to update Todo");
            }
        }

        public async Task<APIResponse> Update(TodoDto todoDto)
        {
            try
            {
                var todoResults = from td in _db.Todos where td.Id == todoDto.Id select td;

                if (todoResults.Any())
                {
                    Todo todo = todoResults.First();
                    todo.Title = todoDto.Title;
                    todo.IsCompleted = todoDto.IsCompleted;
                    todo.Modified = DateTime.Now;
                    await _db.SaveChangesAsync();
                    return new APIResponse(200, "Updated Todo", todo);
                }
                else
                {
                    return new APIResponse(400, "Failed to update Todo");
                }
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to update Todo");
            }
        }

        public async Task<APIResponse> SoftDelete(long id)
        {
            try
            {
                var todoResults = from td in _db.Todos where td.Id == id select td;
                todoResults.First().IsDeleted = true;
                await _db.SaveChangesAsync();
                return new APIResponse(200, "Soft Delete Todo");
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to update Todo");
            }
        }

        public async Task<APIResponse> HardDelete(long id)
        {
            try
            {
                var todoResults = from td in _db.Todos where td.Id == id select td;
                _db.Todos.Remove(todoResults.First());
                await _db.SaveChangesAsync();
                return new APIResponse(200, "Deleted Todo");
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                return new APIResponse(400, "Failed to update Todo");
            }
        }
    }
}
