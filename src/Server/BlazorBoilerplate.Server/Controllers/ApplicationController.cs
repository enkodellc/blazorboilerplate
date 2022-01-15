using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Models;
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
    public class ApplicationController : Controller
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

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<Todo> Todos([FromQuery] ToDoFilter filter)
        {
            return persistenceManager.GetEntities<Todo>().AsNoTracking()
                .Include(i => i.CreatedBy)
                .Include(i => i.ModifiedBy)
                .Where(i =>
                (filter.From == null || i.CreatedOn >= filter.From) && (filter.To == null || i.CreatedOn <= filter.To) &&
                (filter.CreatedById == null || i.CreatedById == filter.CreatedById) &&
                (filter.ModifiedById == null || i.ModifiedById == filter.ModifiedById) &&
                (filter.IsCompleted == null || i.IsCompleted == filter.IsCompleted) &&
                (filter.Query == null || i.Title.ToLower().Contains(filter.Query.ToLower())))
                .OrderByDescending(i => i.CreatedOn);
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<ApplicationUser> TodoCreators([FromQuery] ToDoFilter filter)
        {
            filter.CreatedById = null;

            return Todos(filter).Where(i => i.CreatedBy != null).Select(i => i.CreatedBy).Distinct().AsNoTracking();
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<ApplicationUser> TodoEditors([FromQuery] ToDoFilter filter)
        {
            filter.ModifiedById = null;

            return Todos(filter).Where(i => i.ModifiedBy != null).Select(i => i.ModifiedBy).Distinct().AsNoTracking();
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
