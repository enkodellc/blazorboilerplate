using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using Breeze.Sharp;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface ILocalizationApiClient : IBaseApiClient
    {
        void AddEntity(LocalizationRecord entity);
        void RemoveEntity(LocalizationRecord entity);
        Task<QueryResult<PluralFormRule>> GetPluralFormRules();
        Task<QueryResult<LocalizationRecord>> GetLocalizationRecords(string key = null);
        Task<QueryResult<string>> GetLocalizationRecordMsgIds(int? take, int? skip, string filter = null);
        Task<ApiResponseDto> DeleteLocalizationRecordMsgId(string key);
        Task<ApiResponseDto> EditLocalizationRecordMsgId(string oldkey, string newKey);
        Task<ApiResponseDto> ReloadTranslations();
        Task<ApiResponseDto> Upload(MultipartFormDataContent content);
    }
}
