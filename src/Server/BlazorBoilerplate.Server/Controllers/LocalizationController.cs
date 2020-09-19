using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Models.Localization;
using BlazorBoilerplate.Shared.SqlLocalizer;
using BlazorBoilerplate.Storage;
using Breeze.AspNetCore;
using Breeze.Persistence;
using Breeze.Sharp.Core;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [ApiResponseException]
    [Route("api/localization/[action]")]
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    [BreezeQueryFilter]
    public class LocalizationController : Controller
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + IdentityServerAuthenticationDefaults.AuthenticationScheme; //Cookie + Token authentication

        private LocalizationPersistenceManager persistenceManager;
        private readonly IStringLocalizer<Global> L;
        public LocalizationController(LocalizationPersistenceManager persistenceManager, IStringLocalizer<Global> l)
        {
            this.persistenceManager = persistenceManager;
            L = l;
        }

        [AllowAnonymous]
        [HttpGet]
        public string Metadata()
        {
            return persistenceManager.Metadata();
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<LocalizationRecord> LocalizationRecords(string resourceKey, string key)
        {
            return persistenceManager.GetEntities<LocalizationRecord>()
                .Where(i => (resourceKey == null || i.ResourceKey == resourceKey) && (key == null || i.Key == key)).OrderBy(i => i.LocalizationCulture == Settings.NeutralCulture ? 0 : 1).ThenBy(i => i.LocalizationCulture);
        }

        [HttpGet]
        public IQueryable<string> LocalizationRecordKeys(string resourceKey, string filter)
        {
            return persistenceManager.GetEntities<LocalizationRecord>()
                .Where(i => (resourceKey == null || i.ResourceKey == resourceKey) && (filter == null || i.Key.ToLower().Contains(filter.ToLower()) || i.Text.ToLower().Contains(filter.ToLower())))
                .Distinct(i => i.Key).OrderBy(i => i.Key).Select(i => i.Key).AsQueryable();
        }

        [HttpPost]
        public async Task<ApiResponse> DeleteLocalizationRecordKey([FromBody] LocalizationRecordFilterModel filter)
        {
            int deleted = await persistenceManager.DbContext.Database
                .ExecuteSqlRawAsync("DELETE FROM LocalizationRecords WHERE ResourceKey = {0} AND [Key] = {1}", filter.ResourceKey, filter.Key);

            return new ApiResponse(Status200OK, L["ItemsDeleted", deleted]);
        }

        [HttpPost]
        public async Task<ApiResponse> EditLocalizationRecordKey([FromBody] ChangeLocalizationRecordModel model)
        {
            await persistenceManager.DbContext.Database
                .ExecuteSqlRawAsync("UPDATE LocalizationRecords SET [Key] = {2} WHERE ResourceKey = {0} AND [Key] = {1}", model.ResourceKey, model.Key, model.NewKey);

            return new ApiResponse(Status200OK);
        }

        [HttpPost]
        public ApiResponse ReloadTranslations()
        {
            SqlStringLocalizerFactory.SetLocalizationRecords(persistenceManager.Context.LocalizationRecords);

            return new ApiResponse(Status200OK);
        }

        [HttpPost]
        public SaveResult SaveChanges([FromBody] JObject saveBundle)
        {
            return persistenceManager.SaveChanges(saveBundle);
        }
    }
}
