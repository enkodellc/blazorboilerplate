using AutoMapper.Configuration;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Dto.Sample;
using ApiLogItem = BlazorBoilerplate.Shared.DataModels.ApiLogItem;
using Message = BlazorBoilerplate.Shared.DataModels.Message;
using UserProfile = BlazorBoilerplate.Shared.DataModels.UserProfile;

namespace BlazorBoilerplate.Storage.Mapping
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
