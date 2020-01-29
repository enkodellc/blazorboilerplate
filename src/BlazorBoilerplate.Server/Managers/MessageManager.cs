using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.Dto.Sample;

namespace BlazorBoilerplate.Server.Managers
{
    public class MessageManager : IMessageManager
    {
        private readonly IMessageStore _messageStore;

        public MessageManager(IMessageStore messageStore)
        {
            _messageStore = messageStore;
        }

        public async Task<ApiResponse> Create(MessageDto messageDto)
        {
            var message = await _messageStore.AddMessage(messageDto);
            return new ApiResponse(200, "Created Message", message);
        }

        public async Task<ApiResponse> Delete(int id)
        {
            await _messageStore.DeleteById(id);
            return new ApiResponse(200, "Deleted Message", id);
        }

        public List<MessageDto> GetList()
        {
            return _messageStore.GetMessages();
        }
    }
}
