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
            //.ForMember(t => t.CreatedBy, opt => opt.MapFrom((src, dest, destMember, res) => res.Context.Options.Items["CreatedBy"])); 
            CreateMap<ApiResponse, ApiResponseDto>();
        }
    }
}
