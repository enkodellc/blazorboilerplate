using AutoMapper;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Dto.Sample;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Storage.Stores
{
    public class MessageStore : IMessageStore
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _autoMapper;

        public MessageStore(ApplicationDbContext applicationDbContext,  IMapper autoMapper)
        {
            _applicationDbContext = applicationDbContext;
            _autoMapper = autoMapper;
        }
        
        public async Task<Message> AddMessage(MessageDto messageDto)
        {
            var message = _autoMapper.Map<MessageDto, Message>(messageDto);
            await _applicationDbContext.Messages.AddAsync(message);
            await _applicationDbContext.SaveChangesAsync(CancellationToken.None);

            return message;
        }

        public async Task DeleteById(int id)
        {
            // TODO: Figure out why I was getting an exception when deleting the highest ID. (Apart from that, works)
            _applicationDbContext.Messages.Remove(new Message() { Id = id });
            await _applicationDbContext.SaveChangesAsync(CancellationToken.None);
        }

        public List<MessageDto> GetMessages()
        {
            return _autoMapper.ProjectTo<MessageDto>(_applicationDbContext.Messages).OrderBy(i => i.When).Take(10).ToList();
        }
    }
}