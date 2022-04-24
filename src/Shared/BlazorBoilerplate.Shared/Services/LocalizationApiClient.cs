using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Localization;
using Breeze.Sharp;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplate.Shared.Services
{
    public class LocalizationApiClient : BaseApiClient, ILocalizationApiClient
    {
        public LocalizationApiClient(HttpClient httpClient, ILogger<LocalizationApiClient> logger) : base(httpClient, logger, "api/localization/")
        { }

        public async Task<QueryResult<PluralFormRule>> GetPluralFormRules()
        {
            return await GetItems<PluralFormRule>(from: "PluralFormRules");
        }

        public async Task<QueryResult<LocalizationRecord>> GetLocalizationRecords(LocalizationRecordKey key = null)
        {
            try
            {
                var query = new EntityQuery<LocalizationRecord>().From("LocalizationRecords");

                if (key != null)
                    query = query.WithParameter("contextId", key.ContextId).WithParameter("msgId", key.MsgId);

                var response = await entityManager.ExecuteQuery(query, CancellationToken.None);

                QueryResult<LocalizationRecord> result;

                if (response is QueryResult<LocalizationRecord> result1)
                    result = result1;
                else
                {
                    result = new QueryResult<LocalizationRecord>
                    {
                        Results = response
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError("GetItems: {0}", ex.GetBaseException().Message);

                throw;
            }
        }

        public async Task<QueryResult<LocalizationRecordKey>> GetLocalizationRecordKeys(int? take, int? skip, string filter = null)
        {
            try
            {
                var query = new EntityQuery<LocalizationRecordKey>()
                    .WithParameter("contextId", null).WithParameter("filter", filter).From("LocalizationRecordKeys").InlineCount();

                if (take != null)
                    query = query.Take(take.Value);

                if (skip != null)
                    query = query.Skip(skip.Value);
                else
                    query = query.Skip(0); //query errors if skip is not defined so default to 0


                var response = await entityManager.ExecuteQuery(query, CancellationToken.None);

                QueryResult<LocalizationRecordKey> result;

                if (response is QueryResult<LocalizationRecordKey> result1)
                    result = result1;
                else
                {
                    result = new QueryResult<LocalizationRecordKey>
                    {
                        Results = response
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError("GetItems: {0}", ex.GetBaseException().Message);

                throw;
            }
        }

        public async Task<ApiResponseDto> DeleteLocalizationRecordKey(LocalizationRecordKey key)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}DeleteLocalizationRecordKey", new LocalizationRecordFilterModel { ContextId = key.ContextId, MsgId = key.MsgId });
        }
        public async Task<ApiResponseDto> EditLocalizationRecordKey(LocalizationRecordKey oldKey, LocalizationRecordKey newKey)
        {
            return await httpClient.PostJsonAsync<ApiResponseDto>($"{rootApiPath}EditLocalizationRecordKey",
                new ChangeLocalizationRecordModel { ContextId = oldKey.ContextId, NewContextId = newKey.ContextId, MsgId = oldKey.MsgId, NewMsgId = newKey.MsgId });
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
