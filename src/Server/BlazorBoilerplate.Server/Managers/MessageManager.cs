using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto.Sample;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    [ApiResponseException]
    public class MessageManager : IMessageManager
    {
        private readonly IMessageStore _messageStore;
        private readonly ILogger<MessageManager> _logger;

        public MessageManager(IMessageStore messageStore, ILogger<MessageManager> logger)
        {
            _messageStore = messageStore;
            _logger = logger;
        }

        public async Task<ApiResponse> Create(MessageDto messageDto)
        {
            _logger.LogDebug("Adding message: {@messageDto}", messageDto);
            var message = await _messageStore.AddMessage(messageDto);
            return new ApiResponse(Status200OK, "Created Message", message);
        }

        public async Task<ApiResponse> Delete(int id)
        {
            await _messageStore.DeleteById(id);
            return new ApiResponse(Status200OK, "Deleted Message", id);
        }

        public List<MessageDto> GetList()
        {
            return _messageStore.GetMessages();
        }
    }
}
