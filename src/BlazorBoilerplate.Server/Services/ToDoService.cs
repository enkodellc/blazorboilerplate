using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Services
{
    public interface ITodoService
    {
        Task<ApiResponse> Get();
        Task<ApiResponse> Get(long id);
        Task<ApiResponse> Create(TodoDto todo);
        Task<ApiResponse> Update(TodoDto todo);
        Task<ApiResponse> Delete(long id);
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

        public async Task<ApiResponse> Get()
        {
            try
            {
                //Todo Shadow Property doesn't allow filter of IsDeleted here?
                return new ApiResponse(200, "Retrieved Todos", _autoMapper.ProjectTo<TodoDto>(_db.Todos).ToList());
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, ex.Message);
            }
        }

        public async Task<ApiResponse> Get(long id)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                return new ApiResponse(200, "Retrived Todo", _autoMapper.Map<TodoDto>(todo));
            }
            else
            {
                return new ApiResponse(400, "Failed to Retrieve Todo");
            }
        }

        public async Task<ApiResponse> Create(TodoDto todoDto)
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

            return new ApiResponse(200, "Created Todo", todo);
        }

        public async Task<ApiResponse> Update(TodoDto todoDto)
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
                return new ApiResponse(200, "Updated Todo", todo);
            }
            else
            {
                return new ApiResponse(400, "Failed to update Todo");
            }
        }

        public async Task<ApiResponse> Delete(long id)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                _db.Todos.Remove(todo);
                await _db.SaveChangesAsync();
                return new ApiResponse(200, "Soft Delete Todo");
            }
            else
            {
                return new ApiResponse(400, "Failed to update Todo");
            }
        }
    }
}
