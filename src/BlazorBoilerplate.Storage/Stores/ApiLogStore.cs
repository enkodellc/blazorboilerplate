using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Storage.Stores
{
    public class ApiLogStore : IApiLogStore
    {
        private readonly IApplicationDbContext _db;
        private readonly IMapper _autoMapper;

        public ApiLogStore(IApplicationDbContext db, IMapper autoMapper)
        {
            _db = db;
            _autoMapper = autoMapper;
        }

        public List<ApiLogItemDto> Get()
            => _autoMapper.ProjectTo<ApiLogItemDto>(_db.ApiLogs).ToList();

        public List<ApiLogItemDto> GetByUserId(Guid userId)
        => _autoMapper.ProjectTo<ApiLogItemDto>(_db.ApiLogs.Where(a => a.ApplicationUserId == userId)).ToList();
    }
}