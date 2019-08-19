using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using AutoMapper;

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
        private readonly IMapper _autoMapper;

        public ToDoService(ApplicationDbContext db, IMapper autoMapper)
        {
            _db = db;
            _autoMapper = autoMapper;
        }

        public async Task<APIResponse> Get()
        {
            try
            {
                return new APIResponse(200, "Retrieved Todos", _autoMapper.ProjectTo<TodoDto>(_db.Todos.Where(t => t.IsDeleted == false)).ToList());
            }
            catch (Exception ex)
            {
                return new APIResponse(400, ex.Message);
            }
        }

        public async Task<APIResponse> Get(long id)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                return new APIResponse(200, "Retrived Todo", _autoMapper.Map<TodoDto>(todo));
            }
            else
            {
                return new APIResponse(400, "Failed to Retrieve Todo");
            }
        }

        public async Task<APIResponse> Create(TodoDto todoDto)
        {
            /* Without AutoMapper
            Todo newTodo = new Todo()
            {
                Title = todo.Title,
                IsCompleted = todo.IsCompleted
            };*/

            Todo todo = _autoMapper.Map<TodoDto, Todo>(todoDto);
            await _db.Todos.AddAsync(todo);
            await _db.SaveChangesAsync();

            return new APIResponse(200, "Created Todo", todo);
        }

        public async Task<APIResponse> Update(TodoDto todoDto)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == todoDto.Id);
            if (todo != null)
            {
                /* Without AutoMapper                
                todo.Title = todoDto.Title;
                todo.IsCompleted = todoDto.IsCompleted;
                todo.Modified = DateTime.Now;
                */

                _autoMapper.Map<TodoDto, Todo>(todoDto, todo);
                await _db.SaveChangesAsync();
                return new APIResponse(200, "Updated Todo", todo);
            }
            else
            {
                return new APIResponse(400, "Failed to update Todo");
            }
        }

        public async Task<APIResponse> SoftDelete(long id)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                todo.IsDeleted = true;
                await _db.SaveChangesAsync();
                return new APIResponse(200, "Soft Delete Todo");
            }
            else
            {
                return new APIResponse(400, "Failed to update Todo");
            }
        }

        public async Task<APIResponse> HardDelete(long id)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                _db.Todos.Remove(todo);
                await _db.SaveChangesAsync();
                return new APIResponse(200, "Soft Delete Todo");
            }
            else
            {
                return new APIResponse(400, "Failed to update Todo");
            }
        }
    }
}
