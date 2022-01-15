using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Localization;
using BlazorBoilerplate.Storage;
using Breeze.AspNetCore;
using Breeze.Persistence;
using Breeze.Persistence.EFCore;
using Breeze.Sharp.Core;
using Karambolo.PO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net.Mime;
using System.Text;
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
            "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme; //Cookie + Token authentication

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
            return persistenceManager.GetEntities<PluralFormRule>().AsNoTracking();
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<LocalizationRecord> LocalizationRecords(string contextId, string msgId)
        {
            return persistenceManager.GetEntities<LocalizationRecord>().AsNoTracking()
                .Where(i => (contextId == null || i.ContextId == contextId) && (msgId == null || i.MsgId == msgId))
                .Include(i => i.PluralTranslations)
                .OrderBy(i => i.Culture == Settings.NeutralCulture ? 0 : 1).ThenBy(i => i.Culture);
        }

        [HttpGet]
        public IQueryable<LocalizationRecordKey> LocalizationRecordKeys(string contextId, string filter)
        {
            return persistenceManager.GetEntities<LocalizationRecord>().AsNoTracking()
                .Where(i => (contextId == null || i.ContextId == contextId) && (filter == null || i.MsgId.ToLower().Contains(filter.ToLower()) || i.Translation.ToLower().Contains(filter.ToLower())))
                .Select(i => new LocalizationRecordKey() { MsgId = i.MsgId, ContextId = i.ContextId })
                .Distinct(i => new LocalizationRecordKey() { MsgId = i.MsgId, ContextId = i.ContextId })
                .OrderBy(i => i.ContextId).ThenBy(i => i.MsgId)
                .AsQueryable();
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
        [Authorize(Policies.IsAdmin)]
        public ApiResponse ReloadTranslations()
        {
            localizationProvider.Init(persistenceManager.Context.LocalizationRecords.Include(i => i.PluralTranslations), persistenceManager.Context.PluralFormRules, true);

            return new ApiResponse(Status200OK);
        }

        [HttpPost]
        public SaveResult SaveChanges([FromBody] JObject saveBundle)
        {
            try
            {
                return persistenceManager.SaveChanges(saveBundle);
            }
            catch (Exception ex)
            {
                var errors = new List<EFEntityError>
                {
                    new EFEntityError(null, null, ex.GetBaseException().Message, null)
                };
                throw new EntityErrorsException(errors);
            }
        }

        [HttpGet]
        [Authorize(Policies.IsAdmin)]
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
                        var entries = Enumerable.AsEnumerable<IPOEntry>(textCatalog.Value);

                        var contextIds = entries.Select(i => i.Key.ContextId).Distinct();

                        foreach (var contextId in contextIds)
                        {
                            var filteredCatalog = new POCatalog(entries.Where(entry => entry.Key.ContextId == contextId))
                            {
                                Encoding = textCatalog.Value.Encoding,
                                PluralFormCount = textCatalog.Value.PluralFormCount,
                                PluralFormSelector = textCatalog.Value.PluralFormSelector,
                                Language = textCatalog.Value.Language,
                                Headers = new Dictionary<string, string>
                                {
                                    { "X-Generator", "BlazorBoilerplate" },
                                }
                            };

                            var zipEntry = archive.CreateEntry($"{contextId}-{textCatalog.Key}.po");

                            using var entryStream = zipEntry.Open();
                            generator.Generate(entryStream, filteredCatalog);
                        }
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return File(archiveFile, MediaTypeNames.Application.Zip, "PO.zip");
        }

        [HttpPost]
        [Authorize(Policies.IsAdmin)]
        public async Task<ApiResponse> Upload(IFormFile uploadedFile)
        {
            if (uploadedFile == null || uploadedFile.Length == 0)
                return new ApiResponse(Status404NotFound, L["File not selected"]);

            var parser = new POParser();

            using var stream = new MemoryStream();
            await uploadedFile.CopyToAsync(stream);

            stream.Position = 0;

            string text;

            using (StreamReader reader = new(stream, Encoding.UTF8))
            {
                text = reader.ReadToEnd().Trim('\0');
            }

            var result = parser.Parse(text);

            if (result.Success)
            {
                if (string.IsNullOrWhiteSpace(result.Catalog.Language))
                    result.Catalog.Language = Path.GetFileNameWithoutExtension(uploadedFile.FileName);

                await StorageLocalizationProvider.ImportTextCatalog(persistenceManager.Context, result.Catalog);

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
