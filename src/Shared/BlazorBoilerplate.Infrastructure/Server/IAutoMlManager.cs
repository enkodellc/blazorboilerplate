using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.AutoML;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IAutoMlManager
    {
        Task<ApiResponse> Start(StartAutoMLRequestDto autoMl);
        Task<ApiResponse> GetModel(GetAutoMlModelRequestDto autoMl);
        Task<ApiResponse> TestAutoML(TestAutoMLRequestDto autoMl);
    }
}
