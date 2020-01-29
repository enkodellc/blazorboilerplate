using AutoMapper.Configuration;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Models;
using ApiLogItem = BlazorBoilerplate.Server.Models.ApiLogItem;
using Message = BlazorBoilerplate.Server.Models.Message;
using UserProfile = BlazorBoilerplate.Server.Models.UserProfile;

namespace BlazorBoilerplate.EntityFramework.Mapping
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
