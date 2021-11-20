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

        public async Task<ApiResponse> GetSupportedMlLibraries(GetSupportedMlLibrariesRequestDto task)
        {
            // call grpc method
            GetSupportedMlLibrariesRequest requestGrpc = new GetSupportedMlLibrariesRequest();
            GetSupportedMlLibrariesResponseDto response = new GetSupportedMlLibrariesResponseDto();
            try
            {
                requestGrpc.Task = task.Task;
                var reply = _client.GetSupportedMlLibraries(requestGrpc);
                response.MlLibraries = reply.MlLibraries.ToList();
                return new ApiResponse(Status200OK, null, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }
        public async Task<ApiResponse> GetDatasetCompatibleTasks(GetDatasetCompatibleTasksRequestDto datasetName)
        {
            // call grpc method
            GetDatasetCompatibleTasksRequest requestGrpc = new GetDatasetCompatibleTasksRequest();
            GetDatasetCompatibleTasksResponseDto response = new GetDatasetCompatibleTasksResponseDto();
            try
            {
                requestGrpc.DatasetName = datasetName.DatasetName;
                var reply = _client.GetDatasetCompatibleTasks(requestGrpc);
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
