using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Storage;
using Breeze.AspNetCore;
using Breeze.Persistence;
using Breeze.Persistence.EFCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/data/[action]")]
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    [BreezeQueryFilter]
    public class ApplicationController : BaseController
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme; //Cookie + Token authentication

        private readonly ApplicationPersistenceManager persistenceManager;
        public ApplicationController(ApplicationPersistenceManager persistenceManager)
        {
            this.persistenceManager = persistenceManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public string Metadata()
        {
            return persistenceManager.Metadata();
        }

        [HttpGet]
        public Task<UserProfile> UserProfile()
        {
            return persistenceManager.GetUserProfile();
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<TenantSetting> TenantSettings()
        {
            return persistenceManager.GetEntities<TenantSetting>().AsNoTracking();
        }

        [HttpGet]
        [Authorize(Policies.IsAdmin)]
        public IQueryable<ApplicationUser> Users()
        {
            return persistenceManager.GetEntities<ApplicationUser>().AsNoTracking().Include(i => i.UserRoles).ThenInclude(i => i.Role).OrderBy(i => i.UserName);
        }

        [HttpGet]
        [Authorize(Policies.IsAdmin)]
        public IQueryable<ApplicationRole> Roles()
        {
            return persistenceManager.GetEntities<ApplicationRole>().AsNoTracking().OrderBy(i => i.Name);
        }

        [HttpGet]
        [AuthorizeForFeature(UserFeatures.Operator)]
        public IQueryable<Person> People([FromQuery] string filter)
        {
            filter = filter?.ToLower();

            var userId = GetUserId();

            return persistenceManager.Context.Persons
                .Include(i => i.User)
                .Include(i => i.CreatedBy)
                .Where(i => i.User != null && i.User.Id != userId && (filter == null
                || i.FirstName.ToLower().Contains(filter)
                || i.LastName.ToLower().Contains(filter))).AsNoTracking()
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName);
        }

        [HttpGet]
        [Authorize(Policies.IsAdmin)]
        public IQueryable<DbLog> Logs()
        {
            return persistenceManager.GetEntities<DbLog>().AsNoTracking().OrderByDescending(i => i.TimeStamp);
        }

        [HttpGet]
        [Authorize(Policies.IsAdmin)]
        public IQueryable<ApiLogItem> ApiLogs()
        {
            return persistenceManager.GetEntities<ApiLogItem>().AsNoTracking().OrderByDescending(i => i.RequestTime);
        }

        [AllowAnonymous]
        [HttpPost]
        public SaveResult SaveChanges([FromBody] JObject saveBundle)
        {
            try
            {
                return persistenceManager.SaveChanges(saveBundle);
            }
            catch (EntityErrorsException)
            {
                throw;
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
    }
}
