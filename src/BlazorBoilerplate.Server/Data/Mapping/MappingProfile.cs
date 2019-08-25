using AutoMapper.Configuration;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Server.Data.Mapping
{
    public class MappingProfile : MapperConfigurationExpression
    {
        /// <summary>
        /// Create automap mapping profiles
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Todo, TodoDto>().ReverseMap();           
            CreateMap<UserProfile, UserProfileDto>().ReverseMap();
            CreateMap<ApiLogItem, ApiLogItemDto>().ReverseMap();
            CreateMap<Message, MessageDto>().ReverseMap();
        }
    }
}
