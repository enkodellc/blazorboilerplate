using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Theme.Material.Admin.Shared.Layouts;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin.Settings
{
    [Authorize(Policy = Policies.IsAdmin)]
    [Layout(typeof(AdminLayout))]
    public abstract class SettingsBase : ComponentBase
    {
        protected Dictionary<SettingKey, TenantSetting> settings;

        [Inject]
        protected IApiClient apiClient { get; set; }

        [Inject]
        protected IMatToaster matToaster { get; set; }

        [Inject]
        protected IStringLocalizer<Strings> L { get; set; }

        protected async Task LoadSettings(string prefix)
        {
            try
            {
                var result = (await apiClient.GetTenantSettings()).Where(i => i.Key.ToString().StartsWith(prefix)).ToDictionary(i => i.Key, i => i);

                foreach (var def in TenantSettingValues.Default.Where(i => i.Key.ToString().StartsWith(prefix)))
                {
                    if (!result.ContainsKey(def.Key))
                    {
                        var entity = new TenantSetting() { Key = def.Key, Type = def.Value.Item2, Value = def.Value.Item1 };
                        result.Add(def.Key, entity);
                        apiClient.AddEntity(entity);
                    }
                }

                settings = result;
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        protected async Task SaveChanges()
        {
            try
            {
                await apiClient.SaveChanges();

                matToaster.Add(L["Operation Successful"], MatToastType.Success);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }
    }
}
