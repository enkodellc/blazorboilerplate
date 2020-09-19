using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces.Db;
using Breeze.Sharp;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface ILocalizationApiClient : IBaseApiClient
    {
        void AddEntity(ILocalizationRecord entity);
        void RemoveEntity(ILocalizationRecord entity);
        Task<QueryResult<LocalizationRecord>> GetLocalizationRecords(string key = null);
        Task<QueryResult<string>> GetLocalizationRecordKeys(int? take, int? skip, string filter = null);
        Task<ApiResponseDto> DeleteLocalizationRecordKey(string key);
        Task<ApiResponseDto> EditLocalizationRecordKey(string oldkey, string newKey);
        Task<ApiResponseDto> ReloadTranslations();
    }
}
