using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using Breeze.Sharp;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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
            return await GetItems<Todo>(from: "Todos", orderByDescending: i => i.CreatedOn, take: take, skip: skip, parameters: filter?.ToDictionary());
        }
        public async Task<QueryResult<ApplicationUser>> GetTodoCreators(ToDoFilter filter)
        {
            return await GetItems<ApplicationUser>(from: "TodoCreators", orderBy: i => i.UserName, parameters: filter?.ToDictionary());
        }
        public async Task<QueryResult<ApplicationUser>> GetTodoEditors(ToDoFilter filter)
        {
            return await GetItems<ApplicationUser>(from: "TodoEditors", orderBy: i => i.UserName, parameters: filter?.ToDictionary());
        }
        public async Task<ApiResponseDto> SendTestEmail(EmailDto email)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>("api/Email/SendTestEmail", email);
        }
    }
}
