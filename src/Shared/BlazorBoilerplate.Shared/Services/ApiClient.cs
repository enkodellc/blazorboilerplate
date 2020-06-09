using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using Breeze.Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Services
{
    public class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> logger;

        private readonly DataService dataService;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            this.logger = logger;

            Configuration.Instance.QueryUriStyle = QueryUriStyle.JSON;
            Configuration.Instance.ProbeAssemblies(typeof(ApplicationUser).Assembly);

            var http = new HttpClient();
            foreach (var h in httpClient.DefaultRequestHeaders)
                http.DefaultRequestHeaders.Add(h.Key, h.Value);

            dataService = new DataService(httpClient.BaseAddress + "api/data/", http);
            EntityManager = new EntityManager(dataService);
            var dic = new Dictionary<string, string>();
            dic.Add("BlazorBoilerplate.Infrastructure.Storage.DataModels", "BlazorBoilerplate.Shared.Dto.Db");
            dic.Add("Microsoft.AspNetCore.Identity", "BlazorBoilerplate.Shared.Dto.Db");
            EntityManager.MetadataStore.NamingConvention = new NamingConvention().WithServerClientNamespaceMapping(dic);
        }

        public EntityManager EntityManager { get; }

        public void CancelChanges()
        {
            EntityManager.RejectChanges();
        }

        public async Task SaveChanges()
        {
            try
            {
                await EntityManager.SaveChanges();
            }
            catch (SaveException ex)
            {
                var msg = ex.EntityErrors.First().ErrorMessage;
                logger.LogWarning("SaveChanges: {0}", msg);
                throw new Exception(msg);
            }
            catch (Exception ex)
            {
                logger.LogError("SaveChanges: {0}", ex.GetBaseException().Message);
                throw;
            }
            finally
            {
                EntityManager.RejectChanges();
            }
        }

        public void AddEntity(IEntity entity)
        {
            EntityManager.AddEntity(entity);
        }

        private async Task<QueryResult<T>> GetItems<T>(string from,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            int? take = null,
            int? skip = null)
        {
            try
            {
                var query = new EntityQuery<T>().InlineCount().From(from);

                if (predicate != null)
                    query = query.Where(predicate);

                if (orderBy != null)
                    query = query.OrderBy(orderBy);

                if (take != null)
                    query = query.Take(take.Value);

                if (skip != null)
                    query = query.Skip(skip.Value);

                return (QueryResult<T>)await EntityManager.ExecuteQuery(query, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError("GetItems: {0}", ex.GetBaseException().Message);

                throw;
            }
        }
        public async Task<QueryResult<Todo>> GetToDos()
        {
            return await GetItems<Todo>(from: "Todos", orderBy: i => i.Id);
        }

        public async Task<QueryResult<TenantSetting>> GetTenantSettings()
        {
            return await GetItems<TenantSetting>(from: "TenantSettings", orderBy: i => i.Key);
        }
    }
}
