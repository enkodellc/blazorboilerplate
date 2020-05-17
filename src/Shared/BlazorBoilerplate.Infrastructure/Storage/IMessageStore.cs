using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Dto.Sample;

namespace BlazorBoilerplate.Infrastructure.Storage
{
    public interface IMessageStore
    {
        Task<Message> AddMessage(MessageDto messageDto);

        Task DeleteById(int id);

        List<MessageDto> GetMessages();
    }
}