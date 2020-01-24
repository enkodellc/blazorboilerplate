using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IMessageManager
    {
        Task<ApiResponse> Create(MessageDto messageDto);
        List<MessageDto> GetList();
        Task<ApiResponse> Delete(int id);
    }
}