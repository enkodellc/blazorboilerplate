using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using Breeze.Sharp;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface ILocalizationApiClient : IBaseApiClient
    {
        void AddEntity(LocalizationRecord entity);
        void RemoveEntity(LocalizationRecord entity);
        Task<QueryResult<PluralFormRule>> GetPluralFormRules();
        Task<QueryResult<LocalizationRecord>> GetLocalizationRecords(LocalizationRecordKey key = null);
        Task<QueryResult<LocalizationRecordKey>> GetLocalizationRecordKeys(int? take, int? skip, string filter = null);
        Task<ApiResponseDto> DeleteLocalizationRecordKey(LocalizationRecordKey key);
        Task<ApiResponseDto> EditLocalizationRecordKey(LocalizationRecordKey oldkey, LocalizationRecordKey newKey);
        Task<ApiResponseDto> ReloadTranslations();
        Task<ApiResponseDto> Upload(MultipartFormDataContent content);
    }
}
