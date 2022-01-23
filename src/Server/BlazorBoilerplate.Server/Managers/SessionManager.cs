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
    /// <summary>
    /// Manages all RPC calls related to the AutoML sessions
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ControllerService.ControllerServiceClient _client;
        public SessionManager(ApplicationDbContext dbContext, ControllerService.ControllerServiceClient client)
        {
            _dbContext = dbContext;
            _client = client;
        }
        /// <summary>
        /// Get informations about a specific session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
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
                        Status = (int) automl.Status,
                        Name = automl.Name,
                        Library = automl.Library,
                        Model = automl.Model,
                        TestScore = (double) automl.TestScore,
                        ValidationScore = (double) automl.ValidationScore,
                        Predictiontime = (double) automl.Predictiontime,
                        Runtime = (int)automl.Runtime
                    });
                }
                response.Status = (int)reply.Status;
                response.Dataset = reply.Dataset;
                response.Task = (BlazorBoilerplate.Server.MachineLearningTask)reply.Task;

                response.Configuration = new Shared.Dto.AutoML.AutoMLTabularDataConfiguration();
                response.Configuration.Target = new Shared.Dto.AutoML.AutoMLTarget();
                response.Configuration.Target.Target = reply.TabularConfig.Target.Target;
                response.Configuration.Target.Type = (BlazorBoilerplate.Server.DataType)reply.TabularConfig.Target.Type;

                response.Configuration.Features = new Dictionary<string, BlazorBoilerplate.Server.DataType>();

                foreach (KeyValuePair<string, BlazorBoilerplate.Server.DataType> pair in reply.TabularConfig.Features)
                {

                    response.Configuration.Features.Add(pair.Key, pair.Value);
                }

                foreach(var mllibrarie in reply.RequiredMlLibraries)
                {
                    response.RequiredMlLibraries.Add(mllibrarie);
                }

                foreach(var automl in reply.RequiredAutoMLs)
                {
                    response.RequiredAutoMLs.Add(automl);
                }

                response.RuntimeConstraints = new Shared.Dto.AutoML.AutoMLRuntimeConstraints();
                response.RuntimeConstraints.Runtime_limit = (int)reply.RuntimeConstraints.RuntimeLimit;
                response.RuntimeConstraints.Max_iter = (int)reply.RuntimeConstraints.MaxIter;

                return new ApiResponse(Status200OK, null, response);

            }
            catch (Exception ex)
            {

                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }
        /// <summary>
        /// retrieve all session ids
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
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
