using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.AutoML;
using BlazorBoilerplate.Shared.Dto.Dataset;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Shared.Dto.Ontology;
using BlazorBoilerplate.Shared.Dto.Session;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using Breeze.Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Services
{
    public class ApiClient : BaseApiClient, IApiClient
    {
        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger) : base(httpClient, logger)
        { }
        public async Task<UserProfile> GetUserProfile()
        {
            return (await entityManager.ExecuteQuery(new EntityQuery<UserProfile>().From("UserProfile"), CancellationToken.None)).SingleOrDefault();
        }
        public async Task<QueryResult<TenantSetting>> GetTenantSettings()
        {
            return await GetItems<TenantSetting>(from: "TenantSettings", orderBy: i => i.Key);
        }

        public async Task<QueryResult<ApplicationUser>> GetUsers(Expression<Func<ApplicationUser, bool>> predicate = null, int? take = null, int? skip = null)
        {
            return await GetItems("Users", predicate, i => i.UserName, null, take, skip);
        }
        public async Task<QueryResult<ApplicationRole>> GetRoles(Expression<Func<ApplicationRole, bool>> predicate = null, int? take = null, int? skip = null)
        {
            return await GetItems("Roles", predicate, i => i.Name, null, take, skip);
        }

        public async Task<QueryResult<DbLog>> GetLogs(Expression<Func<DbLog, bool>> predicate = null, int? take = null, int? skip = null)
        {
            return await GetItems("Logs", predicate, null, i => i.TimeStamp, take, skip);
        }

        public async Task<QueryResult<ApiLogItem>> GetApiLogs(Expression<Func<ApiLogItem, bool>> predicate = null, int? take = null, int? skip = null)
        {
            return await GetItems("ApiLogs", predicate, null, i => i.RequestTime, take, skip);
        }
        public async Task<QueryResult<Todo>> GetToDos(ToDoFilter filter, int? take = null, int? skip = null)
        {
            return await GetItems<Todo>(from: "Todos", orderByDescending: i => i.CreatedOn, take: take, skip: skip, parameters: filter.ToDictionary());
        }
        public async Task<QueryResult<ApplicationUser>> GetTodoCreators(ToDoFilter filter)
        {
            return await GetItems<ApplicationUser>(from: "TodoCreators", orderBy: i => i.UserName, parameters: filter.ToDictionary());
        }
        public async Task<QueryResult<ApplicationUser>> GetTodoEditors(ToDoFilter filter)
        {
            return await GetItems<ApplicationUser>(from: "TodoEditors", orderBy: i => i.UserName, parameters: filter.ToDictionary());
        }
        public async Task<ApiResponseDto> SendTestEmail(EmailDto email)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Email/SendTestEmail", email);
        }

        public async Task<ApiResponseDto> GetDatasets()
        {
            return await httpClient.GetJsonAsync<ApiResponseDto>("api/Dataset/GetDatasets");
        }
        public async Task<ApiResponseDto> GetDataset(GetDatasetRequestDto name)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Dataset/GetDataset", name);
        }

        public async Task<ApiResponseDto> UploadDataset(FileUploadRequestDto file)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Dataset/Upload", file);
        }

        public async Task<ApiResponseDto> GetTasks(GetSupportedMlLibrariesRequestDto dataset)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Ontology/GetTasks", dataset);
        }

        public async Task<ApiResponseDto> GetTabularDatasetColumnNames(GetTabularDatasetColumnNamesRequestDto dataset)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Dataset/GetTabularDatasetColumnNames", dataset);
        }

        public async Task<ApiResponseDto> StartAutoML(StartAutoMLRequestDto automl)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/AutoMl/StartAuto", automl);
        }

        public async Task<ApiResponseDto> GetSessions(GetSessionsRequestDto sessions)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Session/GetSessions", sessions);
        }

        public async Task<ApiResponseDto> GetSession(GetSessionRequestDto sessions)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Session/GetSession", sessions);
        }

        public async Task<ApiResponseDto> GetModel(GetAutoMlModelRequestDto automl)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/AutoMl/GetAutoMlModel", automl);
        }

        public async Task<ApiResponseDto> GetCompatibleAutoMlSolutions(GetCompatibleAutoMlSolutionsRequestDto request)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Ontology/GetCompatibleAutoMlSolutions", request);
        }

        public async Task<ApiResponseDto> GetSupportedMlLibraries(GetSupportedMlLibrariesRequestDto task)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Ontology/GetSupportedMlLibraries", task);
        }

        public async Task<ApiResponseDto> GetDatasetCompatibleTasks(GetDatasetCompatibleTasksRequestDto datasetName)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Ontology/GetDatasetCompatibleTasks", datasetName);
        }

        public async Task<ApiResponseDto> TestAutoML(TestAutoMLRequestDto automl)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/AutoMl/TestAutoML", automl);
        }
    }
}
