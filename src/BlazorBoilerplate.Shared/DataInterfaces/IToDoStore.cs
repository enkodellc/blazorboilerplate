using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Sample;

namespace BlazorBoilerplate.Shared.DataInterfaces
{
    public interface IToDoStore
    {
        List<TodoDto> GetAll();
        
        TodoDto GetById(long id);

        Task<Todo> Create(TodoDto todoDto);

        Task<Todo> Update(TodoDto todoDto);

        Task DeleteById(long id);
    }
}