using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Storage;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BlazorBoilerplate.Server.Managers
{
    public class TenantSettingsManager<TSettings> : ITenantSettings<TSettings> where TSettings : class, new()
    {
        private readonly string prefix = $"{typeof(TSettings).Name}_";
        private readonly ApplicationDbContext _dbContext;

        public TenantSettingsManager(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public TSettings Value
        {
            get
            {
                var settings = _dbContext.TenantSettings.AsNoTracking().ToDictionary(i => i.Key, i => i.Value.ToString());
                var tsettings = settings.Concat(TenantSettingValues.Default.ToDictionary(i => i.Key, i => i.Value.Item1)
                    .Where(kvp => !settings.ContainsKey(kvp.Key)))
                    .Where(i => i.Key.ToString().StartsWith(prefix))
                    .ToDictionary(i => i.Key.ToString().Replace(prefix, string.Empty), i => i.Value);

                return JsonConvert.DeserializeObject<TSettings>(JsonConvert.SerializeObject(tsettings));
            }
        }
    }
}
