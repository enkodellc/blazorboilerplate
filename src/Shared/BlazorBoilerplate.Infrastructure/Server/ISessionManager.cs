using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface ISessionManager
    {
        Task<ApiResponse> GetSession(GetSessionRequestDto dataset);
        Task<ApiResponse> GetSessions(GetSessionsRequestDto dataset);
    }
}
