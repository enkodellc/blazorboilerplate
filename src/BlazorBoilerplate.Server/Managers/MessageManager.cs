using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Storage;

namespace BlazorBoilerplate.Server.Managers
{
    public class MessageManager : IMessageManager
    {
        private readonly IApplicationDbContext _db;
        private readonly IMapper _autoMapper;

        public MessageManager(IApplicationDbContext db, IMapper autoMapper)
        {
            _db = db;
            _autoMapper = autoMapper;
        }

        public async Task<ApiResponse> Create(MessageDto messageDto)
        {
            Message message = _autoMapper.Map<MessageDto, Message>(messageDto);
            await _db.Messages.AddAsync(message);
            await _db.SaveChangesAsync(CancellationToken.None);

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
