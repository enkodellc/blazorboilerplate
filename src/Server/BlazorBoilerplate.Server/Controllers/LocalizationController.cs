using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Localization;
using BlazorBoilerplate.Storage;
using Breeze.AspNetCore;
using Breeze.Persistence;
using Breeze.Sharp.Core;
using IdentityServer4.AccessTokenValidation;
using Karambolo.PO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
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

        private readonly LocalizationPersistenceManager persistenceManager;
        private readonly ILocalizationProvider localizationProvider;
        private readonly ILogger<LocalizationController> logger;
        private readonly IStringLocalizer<Global> L;
        public LocalizationController(LocalizationPersistenceManager persistenceManager,
            ILocalizationProvider localizationProvider,
            ILogger<LocalizationController> logger,
            IStringLocalizer<Global> l)
        {
            this.persistenceManager = persistenceManager;
            this.localizationProvider = localizationProvider;
            this.logger = logger;
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
        public IQueryable<PluralFormRule> PluralFormRules()
        {
            return persistenceManager.GetEntities<PluralFormRule>();
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<LocalizationRecord> LocalizationRecords(string contextId, string msgId)
        {
            return persistenceManager.GetEntities<LocalizationRecord>()
                .Where(i => (contextId == null || i.ContextId == contextId) && (msgId == null || i.MsgId == msgId))
                .Include(i => i.PluralTranslations)
                .OrderBy(i => i.Culture == Settings.NeutralCulture ? 0 : 1).ThenBy(i => i.Culture);
        }

        [HttpGet]
        public IQueryable<LocalizationRecordKey> LocalizationRecordKeys(string contextId, string filter)
        {
            return persistenceManager.GetEntities<LocalizationRecord>()
                .Where(i => (contextId == null || i.ContextId == contextId) && (filter == null || i.MsgId.ToLower().Contains(filter.ToLower()) || i.Translation.ToLower().Contains(filter.ToLower())))
                .OrderBy(i => i.ContextId).ThenBy(i => i.MsgId)
                .Select(i => new LocalizationRecordKey() { MsgId = i.MsgId, ContextId = i.ContextId })
                .Distinct(i => new LocalizationRecordKey() { MsgId = i.MsgId, ContextId = i.ContextId }).AsQueryable();
        }

        [HttpPost]
        public async Task<ApiResponse> DeleteLocalizationRecordKey([FromBody] LocalizationRecordFilterModel filter)
        {
            int deleted = await persistenceManager.DbContext.Database
                .ExecuteSqlRawAsync("DELETE FROM LocalizationRecords WHERE ContextId = {0} AND MsgId = {1}", filter.ContextId, filter.MsgId);

            return new ApiResponse(Status200OK, L["ItemsDeleted", deleted]);
        }

        [HttpPost]
        public async Task<ApiResponse> EditLocalizationRecordKey([FromBody] ChangeLocalizationRecordModel model)
        {
            await persistenceManager.DbContext.Database
                .ExecuteSqlRawAsync("UPDATE LocalizationRecords SET ContextId = {2}, MsgId = {3} WHERE ContextId = {0} AND MsgId = {1}",
                model.ContextId, model.MsgId, model.NewContextId, model.NewMsgId);

            return new ApiResponse(Status200OK);
        }

        [HttpPost]
        public ApiResponse ReloadTranslations()
        {
            localizationProvider.Init(persistenceManager.Context.LocalizationRecords.Include(i => i.PluralTranslations), persistenceManager.Context.PluralFormRules, true);

            return new ApiResponse(Status200OK);
        }

        [HttpPost]
        public SaveResult SaveChanges([FromBody] JObject saveBundle)
        {
            return persistenceManager.SaveChanges(saveBundle);
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Zip)]
        public IActionResult Export()
        {
            byte[] archiveFile;

            var generator = new POGenerator();

            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var textCatalog in localizationProvider.TextCatalogs)
                    {
                        var zipEntry = archive.CreateEntry($"{textCatalog.Key}.po");

                        using var entryStream = zipEntry.Open();
                        generator.Generate(entryStream, textCatalog.Value);
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return File(archiveFile, MediaTypeNames.Application.Zip, "PO.zip");
        }

        [HttpPost]
        public async Task<ApiResponse> Upload(IFormFile uploadedFile)
        {
            if (uploadedFile == null || uploadedFile.Length == 0)
                return new ApiResponse(Status404NotFound, L["File not selected"]);

            var parser = new POParser();

            using (var stream = new MemoryStream())
            {
                await uploadedFile.CopyToAsync(stream);

                stream.Position = 0;

                var result = parser.Parse(new StreamReader(stream));

                if (result.Success)
                {
                    if (string.IsNullOrWhiteSpace(result.Catalog.Language))
                        result.Catalog.Language = Path.GetFileNameWithoutExtension(uploadedFile.FileName);

                    await ((StorageLocalizationProvider)localizationProvider).ImportTextCatalog(persistenceManager.Context, result.Catalog);

                    logger.LogInformation($"File {uploadedFile.FileName} uploaded by {User.Identity.Name} and imported successfully");

                    return new ApiResponse(Status200OK);
                }
                else
                {
                    return new ApiResponse(Status400BadRequest, L["File not valid"]);
                }
            }
        }
    }
}
