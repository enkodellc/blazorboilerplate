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
    public class OntologyManager : IOntologyManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ControllerService.ControllerServiceClient _client;
        public OntologyManager(ApplicationDbContext dbContext, ControllerService.ControllerServiceClient client)
        {
            _dbContext = dbContext;
            _client = client;
        }
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
