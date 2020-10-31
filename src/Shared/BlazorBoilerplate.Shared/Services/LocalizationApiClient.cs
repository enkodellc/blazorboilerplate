using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Localization;
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

        public async Task<QueryResult<PluralFormRule>> GetPluralFormRules()
        {
            return await GetItems<PluralFormRule>(from: "PluralFormRules");
        }

        public async Task<QueryResult<LocalizationRecord>> GetLocalizationRecords(string msgId = null)
        {
            try
            {
                var query = new EntityQuery<LocalizationRecord>()
                    .WithParameter("contextId", nameof(Global)).WithParameter("msgId", msgId).From("LocalizationRecords");

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

        public async Task<QueryResult<string>> GetLocalizationRecordMsgIds(int? take, int? skip, string filter = null)
        {
            try
            {
                var query = new EntityQuery<string>()
                    .WithParameter("contextId", nameof(Global)).WithParameter("filter", filter).From("LocalizationRecordMsgIds").InlineCount();

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

        public async Task<ApiResponseDto> DeleteLocalizationRecordMsgId(string msgId)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}DeleteLocalizationRecordMsgId", new LocalizationRecordFilterModel { ContextId = nameof(Global), MsgId = msgId });
        }
        public async Task<ApiResponseDto> EditLocalizationRecordMsgId(string oldMsgId, string newMsgId)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}EditLocalizationRecordMsgId", new ChangeLocalizationRecordModel { ContextId = nameof(Global), MsgId = oldMsgId, NewMsgId = newMsgId });
        }
        public async Task<ApiResponseDto> ReloadTranslations()
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}ReloadTranslations", null);
        }

        public void AddEntity(LocalizationRecord localizationRecord)
        {
            AddEntity((IEntity)localizationRecord);
        }

        public void RemoveEntity(LocalizationRecord localizationRecord)
        {
            RemoveEntity((IEntity)localizationRecord);
        }
        public async Task<ApiResponseDto> Upload(MultipartFormDataContent content)
        {
            return await httpClient.PostFileAsync<ApiResponseDto>($"{rootApiPath}Upload", content);
        }
    }
}
