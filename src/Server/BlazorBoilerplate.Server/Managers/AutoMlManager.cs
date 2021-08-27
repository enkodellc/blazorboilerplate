﻿using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.AutoML;
using BlazorBoilerplate.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Google.Protobuf; 

namespace BlazorBoilerplate.Server.Managers
{
    public class AutoMlManager : IAutoMlManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ControllerService.ControllerServiceClient _client;
        public AutoMlManager(ApplicationDbContext dbContext, ControllerService.ControllerServiceClient client)
        {
            _dbContext = dbContext;
            _client = client;
        }

        public async Task<ApiResponse> GetModel(GetAutoMlModelRequestDto autoMl)
        {
            GetAutoMlModelRespomseDto response = new GetAutoMlModelRespomseDto();
            GetAutoMlModelRequest getmodelRequest = new GetAutoMlModelRequest();
            try
            {
                getmodelRequest.SessionId = autoMl.SessionId;
                getmodelRequest.AutoMl = autoMl.AutoMl;
                var reply = _client.GetAutoMlModel(getmodelRequest);
                response.Name = reply.Name;
                response.Content = reply.File.ToByteArray();
                return new ApiResponse(Status200OK, null, response);

            }
            catch (Exception ex)
            {

                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }

        public async Task<ApiResponse> Start(StartAutoMLRequestDto autoMl)
        {
            StartAutoMLResponseDto response = new StartAutoMLResponseDto();
            StartAutoMLprocessRequest getDatasetRequest = new StartAutoMLprocessRequest();
            try
            {
                getDatasetRequest.Dataset = autoMl.DatasetName;
                getDatasetRequest.Task = GetMachineLearningTask(autoMl);
                getDatasetRequest.TabularConfig = GetTabularDataConfiguration(autoMl);
                var reply = _client.StartAutoMLprocess(getDatasetRequest);
                if (reply.Result == ControllerReturnCode.Success)
                {
                    response.SessionId = reply.SessionId;
                    return new ApiResponse(Status200OK, null, response);
                }
                else
                {
                    return new ApiResponse(Status400BadRequest, "Error while starting AutoML Code: " + reply.Result + "", null);
                }

            }
            catch (Exception ex)
            {

                return new ApiResponse(Status404NotFound, ex.Message);
            }
        }
        private MachineLearningTask GetMachineLearningTask(StartAutoMLRequestDto autoMl)
        {
            switch (autoMl.DatasetType)
            {
                case "TABULAR":
                    switch (autoMl.Task)
                    {
                        case "classification":
                            return MachineLearningTask.TabularClassification;
                        case "regression":
                            return MachineLearningTask.TabularRegression;
                        default:
                            return MachineLearningTask.Unknown;
                    }
                default:
                    return MachineLearningTask.Unknown;
            }
        }
        private AutoMLConfigurationTabularData GetTabularDataConfiguration(StartAutoMLRequestDto autoMl)
        {
            switch (autoMl.DatasetType)
            {
                case "TABULAR":
                    AutoMLConfigurationTabularData conf = new AutoMLConfigurationTabularData();
                    autoMl.Configuration = ((JObject)autoMl.Configuration).ToObject<AutoMLTabularDataConfiguration>();
                    conf.Target = ((AutoMLTabularDataConfiguration)autoMl.Configuration).Target;
                    return conf;
                default:
                    return null;
            }
        }
    }
}
