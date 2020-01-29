using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Sample;
using BlazorBoilerplate.Storage;

namespace BlazorBoilerplate.Server.Managers
{
    public class MessageManager : IMessageManager
    {
        private readonly IApplicationDbContext _db;
        private readonly IMapper _autoMapper;
        private readonly IMessageStore _messageStore;

        public MessageManager(IApplicationDbContext db, IMapper autoMapper, IMessageStore messageStore)
        {
            _db = db;
            _autoMapper = autoMapper;
            _messageStore = messageStore;
        }

        public async Task<ApiResponse> Create(MessageDto messageDto)
        {
            var message = await _messageStore.AddMessage(messageDto);
            return new ApiResponse(200, "Created Message", message);
        }

        public async Task<ApiResponse> Delete(int id)
        {
            _db.Messages.Remove(new Message() { Id = id });
            await _db.SaveChangesAsync(CancellationToken.None);

            return new ApiResponse(200, "Deleted Message", id);
        }

        public List<MessageDto> GetList()
        {
            return _autoMapper.ProjectTo<MessageDto>(_db.Messages).OrderBy(i => i.When).Take(10).ToList();
        }
    }
}
