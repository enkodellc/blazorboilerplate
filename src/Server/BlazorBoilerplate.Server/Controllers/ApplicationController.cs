using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Storage;
using Breeze.AspNetCore;
using Breeze.Persistence;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/data/[action]")]
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    [BreezeQueryFilter]
    public class ApplicationController : Controller
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + IdentityServerAuthenticationDefaults.AuthenticationScheme; //Cookie + Token authentication

        private ApplicationPersistenceManager persistenceManager;
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
            return persistenceManager.GetEntities<TenantSetting>();
        }

        [HttpGet]
        public IQueryable<ApplicationUser> Users()
        {
            return persistenceManager.GetEntities<ApplicationUser>().Include(i => i.UserRoles).ThenInclude(i => i.Role).OrderBy(i => i.UserName);
        }

        [HttpGet]
        public IQueryable<ApplicationRole> Roles()
        {
            return persistenceManager.GetEntities<ApplicationRole>().OrderBy(i => i.Name);
        }

        [AllowAnonymous]
        [HttpGet]
        public IQueryable<Todo> Todos()
        {
            return persistenceManager.GetEntities<Todo>().Include(i => i.CreatedBy).Include(i => i.ModifiedBy).OrderBy(i => i.Id);
        }

        [HttpGet]
        public IQueryable<DbLog> Logs()
        {
            return persistenceManager.GetEntities<DbLog>().AsNoTracking().OrderByDescending(i => i.TimeStamp);
        }

        [HttpGet]
        public IQueryable<ApiLogItem> ApiLogs()
        {
            return persistenceManager.GetEntities<ApiLogItem>().AsNoTracking().OrderByDescending(i => i.RequestTime);
        }

        [AllowAnonymous]
        [HttpPost]
        public SaveResult SaveChanges([FromBody] JObject saveBundle)
        {
            return persistenceManager.SaveChanges(saveBundle);
        }
    }
}
