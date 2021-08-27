using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Session;
using BlazorBoilerplate.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class SessionManager : ISessionManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ControllerService.ControllerServiceClient _client;
        public SessionManager(ApplicationDbContext dbContext, ControllerService.ControllerServiceClient client)
        {
            _dbContext = dbContext;
            _client = client;
        }

        public async Task<ApiResponse> GetSession(GetSessionRequestDto session)
        {
            GetSessionStatusRequest request = new GetSessionStatusRequest();
            GetSessionResponseDto response = new GetSessionResponseDto();
            try
            {
                request.Id = session.SessionId;
                var reply = _client.GetSessionStatus(request);
                foreach (var automl in reply.Automls)
                {
                    response.AutoMls.Add(new Shared.Dto.AutoML.AutoMLStatusDto
                    {
                        Messages = automl.Messages.ToList(),
                        Status = (int)automl.Status,
                        Name = automl.Name
                    });
                }
                response.Status = (int)reply.Status;
                return new ApiResponse(Status200OK, null, response);

            }
            catch (Exception ex)
            {

                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }

        public async Task<ApiResponse> GetSessions(GetSessionsRequestDto dataset)
        {
            GetSessionsRequest request = new GetSessionsRequest();
            GetSessionsResponseDto response = new GetSessionsResponseDto();
            try
            {
                request.User = ""; //TODO USER MANAGERMENT
                var reply = _client.GetSessions(request);
                response.SessionIds = reply.SessionIds.ToList();
                return new ApiResponse(Status200OK, null, response);

            }
            catch (Exception ex)
            {

                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }
    }
}
