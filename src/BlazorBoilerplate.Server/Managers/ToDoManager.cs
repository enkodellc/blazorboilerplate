using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Sample;
using BlazorBoilerplate.Storage;

namespace BlazorBoilerplate.Server.Managers
{
    public class ToDoManager : ITodoManager
    {
        private readonly IApplicationDbContext _db;
        private readonly IMapper _autoMapper;

        public ToDoManager(IApplicationDbContext db, IMapper autoMapper)
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
            await _db.SaveChangesAsync(CancellationToken.None);

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
                await _db.SaveChangesAsync(CancellationToken.None);
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
                await _db.SaveChangesAsync(CancellationToken.None);
                return new ApiResponse(200, "Soft Delete Todo");
            }
            else
            {
                return new ApiResponse(400, "Failed to update Todo");
            }
        }
    }
}
