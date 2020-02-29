using System;
using System.Collections.Generic;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Shared.DataInterfaces
{
    public interface IApiLogStore
    {
        List<ApiLogItemDto> Get();

        List<ApiLogItemDto> GetByUserId(Guid userId);
    }
}