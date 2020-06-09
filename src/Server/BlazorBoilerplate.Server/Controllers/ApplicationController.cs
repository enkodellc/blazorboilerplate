using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Storage;
using Breeze.AspNetCore;
using Breeze.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/data/[action]")]
    [BreezeQueryFilter]
    public class ApplicationController : Controller
    {
        private ApplicationPersistenceManager persistenceManager;
        public ApplicationController(ApplicationPersistenceManager persistenceManager)
        {
            this.persistenceManager = persistenceManager;
        }

        [HttpGet]
        public string Metadata()
        {
            return persistenceManager.Metadata();
        }

        [HttpGet]
        public IQueryable<TenantSetting> TenantSettings()
        {
            return persistenceManager.GetEntities<TenantSetting>();
        }

        [HttpGet]
        public IQueryable<Todo> Todos()
        {
            return persistenceManager.GetEntities<Todo>().Include(i => i.CreatedBy).Include(i => i.ModifiedBy).OrderBy(i => i.Id);
        }

        [HttpPost]
        public SaveResult SaveChanges([FromBody] JObject saveBundle)
        {
            return persistenceManager.SaveChanges(saveBundle);
        }
    }
}
