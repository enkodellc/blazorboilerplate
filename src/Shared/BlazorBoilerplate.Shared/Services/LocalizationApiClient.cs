using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces.Db;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Localization;
using BlazorBoilerplate.Shared.SqlLocalizer;
using Breeze.Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Services
{
    public class LocalizationApiClient : BaseApiClient, ILocalizationApiClient
    {
        private const string rootApiPath = "api/localization/";
        public LocalizationApiClient(HttpClient httpClient, ILogger<LocalizationApiClient> logger) : base(httpClient, logger, rootApiPath)
        { }

        public async Task<QueryResult<LocalizationRecord>> GetLocalizationRecords(string key = null)
        {
            try
            {
                var query = new EntityQuery<LocalizationRecord>()
                    .WithParameter("resourceKey", nameof(Global)).WithParameter("key", key).From("LocalizationRecords");

                var response = await entityManager.ExecuteQuery(query, CancellationToken.None);

                QueryResult<LocalizationRecord> result;

                if (response is QueryResult<LocalizationRecord>)
                    result = (QueryResult<LocalizationRecord>)response;
                else
                {
                    result = new QueryResult<LocalizationRecord>();
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

        public async Task<QueryResult<string>> GetLocalizationRecordKeys(int? take, int? skip, string filter = null)
        {
            try
            {
                var query = new EntityQuery<string>()
                    .WithParameter("resourceKey", nameof(Global)).WithParameter("filter", filter).From("LocalizationRecordKeys").InlineCount();

                if (take != null)
                    query = query.Take(take.Value);

                if (skip != null)
                    query = query.Skip(skip.Value);


                var response = await entityManager.ExecuteQuery(query, CancellationToken.None);

                QueryResult<string> result;

                if (response is QueryResult<string>)
                    result = (QueryResult<string>)response;
                else
                {
                    result = new QueryResult<string>();
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

        public async Task<ApiResponseDto> DeleteLocalizationRecordKey(string key)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}DeleteLocalizationRecordKey", new LocalizationRecordFilterModel { ResourceKey = nameof(Global), Key = key });
        }
        public async Task<ApiResponseDto> EditLocalizationRecordKey(string oldkey, string newKey)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}EditLocalizationRecordKey", new ChangeLocalizationRecordModel { ResourceKey = nameof(Global), Key = oldkey, NewKey = newKey });
        }
        public async Task<ApiResponseDto> ReloadTranslations()
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}ReloadTranslations", null);
        }

        public void AddEntity(ILocalizationRecord localizationRecord)
        {
            base.AddEntity((IEntity)localizationRecord);
        }

        public void RemoveEntity(ILocalizationRecord localizationRecord)
        {
            base.RemoveEntity((IEntity)localizationRecord);
        }
    }
}
