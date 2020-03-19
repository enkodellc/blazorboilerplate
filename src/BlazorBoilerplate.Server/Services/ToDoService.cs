using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.EntityFrameworkCore;
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
                return new ApiResponse(Status200OK, "Retrieved Todos", await _autoMapper.ProjectTo<TodoDto>(_db.Todos).ToListAsync());
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }

        public async Task<ApiResponse> Get(long id)
        {
            Todo todo = await _db.Todos.FirstOrDefaultAsync(t => t.Id == id);
            if (todo != null)
            {
                return new ApiResponse(Status200OK, "Retrieved Todo", _autoMapper.Map<TodoDto>(todo));
            }
            else
            {
                return new ApiResponse(Status400BadRequest, "Failed to Retrieve Todo");
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

            return new ApiResponse(Status200OK, "Created Todo", todo);
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
                return new ApiResponse(Status200OK, "Updated Todo", todo);
            }
            else
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Todo");
            }
        }

        public async Task<ApiResponse> Delete(long id)
        {
            Todo todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                _db.Todos.Remove(todo);
                await _db.SaveChangesAsync();
                return new ApiResponse(Status200OK, "Soft Delete Todo");
            }
            else
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Todo");
            }
        }
    }
}
