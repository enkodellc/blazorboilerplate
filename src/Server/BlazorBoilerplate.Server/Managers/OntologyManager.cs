using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Ontology;
using BlazorBoilerplate.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    /// <summary>
    /// Manages all RPC calls which are connected to requests for knowledge from the Ontologie
    /// </summary>
    public class OntologyManager : IOntologyManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ControllerService.ControllerServiceClient _client;
        public OntologyManager(ApplicationDbContext dbContext, ControllerService.ControllerServiceClient client)
        {
            _dbContext = dbContext;
            _client = client;
        }

        public async Task<ApiResponse> GetCompatibleAutoMlSolutions(GetCompatibleAutoMlSolutionsRequestDto request)
        {
            // call grpc method
            GetCompatibleAutoMlSolutionsRequest requestGrpc = new GetCompatibleAutoMlSolutionsRequest();
            GetCompatibleAutoMlSolutionsResponseDto response = new GetCompatibleAutoMlSolutionsResponseDto();
            try
            {
                requestGrpc.Configuration.Add(request.Configuration);
                var reply = _client.GetCompatibleAutoMlSolutions(requestGrpc);
                response.AutoMlSolutions = reply.AutoMlSolutions.ToList();
                return new ApiResponse(Status200OK, null, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }

        /// <summary>
        /// Query for all supports tasks
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public async Task<ApiResponse> GetTasks(GetTasksRequestDto dataset)
        {
            GetTasksRequest request = new GetTasksRequest();
            GetTasksResponseDto response = new GetTasksResponseDto();
            try
            {
                request.DatasetName = dataset.Dataset;
                var reply = _client.GetTasks(request);
                response.Tasks = reply.Tasks.ToList();
                return new ApiResponse(Status200OK, null, response);

            }
            catch (Exception ex)
            {

                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }
    }
}
