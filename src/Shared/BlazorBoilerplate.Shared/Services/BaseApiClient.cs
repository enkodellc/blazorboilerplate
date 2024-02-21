using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using Breeze.Sharp;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BlazorBoilerplate.Shared.Services
{
    public abstract class BaseApiClient : IBaseApiClient
    {
        protected readonly ILogger<BaseApiClient> logger;

        protected readonly HttpClient httpClient;

        protected readonly DataService dataService;

        protected readonly EntityManager entityManager;

        protected readonly string rootApiPath;

        public BaseApiClient(HttpClient httpClient, ILogger<BaseApiClient> logger, string rootApiPath = "api/data/")
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.rootApiPath = rootApiPath;

            Configuration.Instance.QueryUriStyle = QueryUriStyle.JSON;
            Configuration.Instance.ProbeAssemblies(typeof(ApplicationUser).Assembly);

            dataService = new DataService(httpClient.BaseAddress + rootApiPath, httpClient);
            entityManager = new EntityManager(dataService);

            var clientNameSpace = typeof(ApplicationUser).Namespace;
            var dic = new Dictionary<string, string>();
            dic.Add("BlazorBoilerplate.Infrastructure.Storage.DataModels", clientNameSpace);
            dic.Add("Microsoft.AspNetCore.Identity", clientNameSpace);

            entityManager.MetadataStore.NamingConvention = new NamingConvention().WithServerClientNamespaceMapping(dic);

            entityManager.MetadataStore.AllowedMetadataMismatchTypes = MetadataMismatchTypes.MissingCLRDataProperty;

            entityManager.FetchMetadata().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var message = t.Exception.GetBaseException().ToString();

                    var breezeException = t.Exception.GetBaseException().InnerException as DataServiceRequestException;

                    if (breezeException != null)
                        message = breezeException.Message;

                    if (breezeException?.HttpResponse.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                        logger.LogError("BaseApiClient.FetchMetadata: {0}", message);
                }
            });
        }

        protected static Expression<Func<T, object>> GenerateExpression<T>(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;
            else
            {
                var param = Expression.Parameter(typeof(T), "i");

                Expression body = param;

                foreach (var member in propertyName.Split('.'))
                    body = Expression.PropertyOrField(body, member);

                return Expression.Lambda<Func<T, object>>(Expression.Convert(body, typeof(object)), param);
            }
        }

        public void ClearEntitiesCache()
        {
            entityManager.Clear();
        }

        public void CancelChanges()
        {
            entityManager.RejectChanges();
        }

        public event EventHandler<EntityChangedEventArgs> EntityChanged
        {
            add
            {
                entityManager.EntityChanged += value;

            }
            remove
            {
                entityManager.EntityChanged -= value;

            }
        }

        public async Task SaveChanges()
        {
            try
            {
                await entityManager.SaveChanges();
            }
            catch (SaveException ex)
            {
                var msg = ex.EntityErrors.FirstOrDefault()?.ErrorMessage;

                if (msg == null)
                    msg = ex.ValidationErrors.FirstOrDefault()?.Message;

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
        public async Task<QueryResult<T>> GetItems<T>(string from,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            Expression<Func<T, object>> orderByDescending = null,
            int? take = null,
            int? skip = null,
            Dictionary<string, object> parameters = null)
        {
            try
            {
                var query = new EntityQuery<T>().InlineCount().From(from);

                if (parameters != null)
                    query = query.WithParameters(parameters);

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
                else
                    query = query.Skip(0); //query errors if skip is not defined so default to 0

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

        public async Task<QueryResult<T>> GetItemsByFilter<T>(
            string from,
            string orderByDefaultField,
            string filter = null,
            string orderBy = null,
            string orderByDescending = null,
            int? take = null, int? skip = null,
            Dictionary<string, object> parameters = null)
        {
            if (orderBy == null && orderByDescending == null)
                orderBy = orderByDefaultField;

            if (!string.IsNullOrWhiteSpace(filter) && parameters == null)
                parameters = new() { { "filter", filter } };

            return await GetItems(from, null, GenerateExpression<T>(orderBy), GenerateExpression<T>(orderByDescending), take, skip, parameters);
        }
    }
}
