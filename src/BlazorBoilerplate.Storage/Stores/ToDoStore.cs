using AutoMapper;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Sample;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Storage.Stores
{
    public class ToDoStore : IToDoStore
    {
        private readonly IApplicationDbContext _db;
        private readonly IMapper _autoMapper;

        public ToDoStore(IApplicationDbContext db, IMapper autoMapper)
        {
            _db = db;
            _autoMapper = autoMapper;
        }       

        public async Task<List<TodoDto>> GetAll()
        {
            return await _autoMapper.ProjectTo<TodoDto>(_db.Todos).ToListAsync();
        }

        public async Task<TodoDto> GetById(long id)
        {
            var todo = await _db.Todos.SingleOrDefaultAsync(t => t.Id == id);

            if (todo == null)
                throw new InvalidDataException($"Unable to find Todo with ID: {id}");

            return _autoMapper.Map<TodoDto>(todo);
        }

        public async Task<Todo> Create(TodoDto todoDto)
        {
            var todo = _autoMapper.Map<TodoDto, Todo>(todoDto);
            await _db.Todos.AddAsync(todo);
            await _db.SaveChangesAsync(CancellationToken.None);
            return todo;
        }

        public async Task<Todo> Update(TodoDto todoDto)
        {
            var todo = await _db.Todos.SingleOrDefaultAsync(t => t.Id == todoDto.Id);
            if (todo == null)
                throw new InvalidDataException($"Unable to find Todo with ID: {todoDto.Id}");

            todo = _autoMapper.Map(todoDto, todo);
            _db.Todos.Update(todo);
            await _db.SaveChangesAsync(CancellationToken.None);

            return todo;
        }

        public async Task DeleteById(long id)
        {
            var todo = _db.Todos.SingleOrDefault(t => t.Id == id);

            if (todo == null)
                throw new InvalidDataException($"Unable to find Todo with ID: {id}");

            _db.Todos.Remove(todo);
            await _db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
