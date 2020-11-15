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
    public abstract class BaseApiClient : IBaseApiClient
    {
        protected readonly ILogger<BaseApiClient> logger;

        protected readonly HttpClient httpClient;

        protected readonly DataService dataService;

        protected readonly EntityManager entityManager;

        public BaseApiClient(HttpClient httpClient, ILogger<BaseApiClient> logger, string rootApiPath = "api/data/")
        {
            this.logger = logger;
            this.httpClient = httpClient;

            Configuration.Instance.QueryUriStyle = QueryUriStyle.JSON;
            Configuration.Instance.ProbeAssemblies(typeof(ApplicationUser).Assembly);

            dataService = new DataService(httpClient.BaseAddress + rootApiPath, httpClient);
            entityManager = new EntityManager(dataService);

            var clientNameSpace = typeof(ApplicationUser).Namespace;
            var dic = new Dictionary<string, string>();
            dic.Add("BlazorBoilerplate.Infrastructure.Storage.DataModels", clientNameSpace);
            dic.Add("Microsoft.AspNetCore.Identity", clientNameSpace);

            entityManager.MetadataStore.NamingConvention = new NamingConvention().WithServerClientNamespaceMapping(dic);

            entityManager.FetchMetadata().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    logger.LogError("FetchMetadata: {0}", t.Exception.GetBaseException());
            });
        }

        public void ClearEntitiesCache()
        {
            entityManager.Clear();
        }

        public void CancelChanges()
        {
            entityManager.RejectChanges();
        }

        public async Task SaveChanges()
        {
            try
            {
                await entityManager.SaveChanges();
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
                entityManager.RejectChanges();
            }
        }

        public void AddEntity(IEntity entity)
        {
            entityManager.AddEntity(entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            entity.EntityAspect.Delete();
        }
        protected async Task<QueryResult<T>> GetItems<T>(string from,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            Expression<Func<T, object>> orderByDescending = null,
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

                if (orderByDescending != null)
                    query = query.OrderByDescending(orderByDescending);

                if (take != null)
                    query = query.Take(take.Value);

                if (skip != null)
                    query = query.Skip(skip.Value);

                var response = await entityManager.ExecuteQuery(query, CancellationToken.None);

                QueryResult<T> result;

                if (response is QueryResult<T>)
                    result = (QueryResult<T>)response;
                else
                {
                    result = new QueryResult<T>();
                    result.Results = response;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError("GetItems: {0}", ex.GetBaseException().Message);

                throw;
            }
        }
    }
}
