using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Sample;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IMessageManager
    {
        Task<ApiResponse> Create(MessageDto messageDto);
        List<MessageDto> GetList();
        Task<ApiResponse> Delete(int id);
    }
}