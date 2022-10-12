using AutoMapper;
using AutoMapper.Configuration;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Dto.Sample;
using Finbuckle.MultiTenant;
using Message = BlazorBoilerplate.Infrastructure.Storage.DataModels.Message;

namespace BlazorBoilerplate.Storage.Mapping
{
    public class MappingProfile : MapperConfigurationExpression
    {
        /// <summary>
        /// Create automap mapping profiles
        /// </summary>
        public MappingProfile()
        {
            CreateMap<TenantInfo, TenantDto>().ReverseMap();
            CreateMap<Message, MessageDto>().ReverseMap();
        }
    }
}
