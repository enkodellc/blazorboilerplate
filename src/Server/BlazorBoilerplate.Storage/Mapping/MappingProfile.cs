using AutoMapper;
using BlazorBoilerplate.Shared.Dto.Admin;
using Finbuckle.MultiTenant;

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
        }
    }
}
