using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Shared.Models;
using Breeze.Sharp;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Dto.Dataset;
using BlazorBoilerplate.Shared.Dto.Ontology;
using BlazorBoilerplate.Shared.Dto.AutoML;
using BlazorBoilerplate.Shared.Dto.Session;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IApiClient : IBaseApiClient
    {
        Task<UserProfile> GetUserProfile();

        Task<QueryResult<TenantSetting>> GetTenantSettings();
        Task<QueryResult<ApplicationUser>> GetUsers(Expression<Func<ApplicationUser, bool>> predicate = null, int? take = null, int? skip = null);
        Task<QueryResult<ApplicationRole>> GetRoles(Expression<Func<ApplicationRole, bool>> predicate = null, int? take = null, int? skip = null);

        Task<QueryResult<DbLog>> GetLogs(Expression<Func<DbLog, bool>> predicate = null, int? take = null, int? skip = null);
        Task<QueryResult<ApiLogItem>> GetApiLogs(Expression<Func<ApiLogItem, bool>> predicate = null, int? take = null, int? skip = null);

        Task<QueryResult<Todo>> GetToDos(ToDoFilter filter, int? take = null, int? skip = null);
        Task<QueryResult<ApplicationUser>> GetTodoCreators(ToDoFilter filter);
        Task<QueryResult<ApplicationUser>> GetTodoEditors(ToDoFilter filter);

        Task<ApiResponseDto> SendTestEmail(EmailDto email);

        Task<ApiResponseDto> GetModel(GetAutoMlModelRequestDto automl);
        Task<ApiResponseDto> GetDatasets();
        Task<ApiResponseDto> GetDataset(GetDatasetRequestDto name);
      
        Task<ApiResponseDto> GetTabularDatasetColumnNames(GetTabularDatasetColumnNamesRequestDto dataset);
        Task<ApiResponseDto> StartAutoML(StartAutoMLRequestDto automl);
        Task<ApiResponseDto> UploadDataset(FileUploadRequestDto file);
        Task<ApiResponseDto> GetSessions(GetSessionsRequestDto sessions);
        Task<ApiResponseDto> GetSession(GetSessionRequestDto sessions);

        Task<ApiResponseDto> GetCompatibleAutoMlSolutions(GetCompatibleAutoMlSolutionsRequestDto request);
        Task<ApiResponseDto> GetSupportedMlLibraries(GetSupportedMlLibrariesRequestDto task);
        Task<ApiResponseDto> GetDatasetCompatibleTasks(GetDatasetCompatibleTasksRequestDto datasetName);
        Task<ApiResponseDto> TestAutoML(TestAutoMLRequestDto datasetName);
    }
}
