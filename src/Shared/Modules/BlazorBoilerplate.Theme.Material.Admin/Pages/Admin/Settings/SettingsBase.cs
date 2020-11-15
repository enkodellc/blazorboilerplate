using BlazorBoilerplate.Shared.Localizer;
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
using BlazorBoilerplate.Constants;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin.Settings
{
    [Authorize(Policy = Policies.IsAdmin)]
    [Layout(typeof(AdminLayout))]
    public abstract class SettingsBase : ComponentBase
    {
        protected Dictionary<SettingKey, TenantSetting> settings;

        [Inject] private NavigationManager navigationManager { get; set; }

        [Inject] protected IApiClient apiClient { get; set; }

        [Inject] protected IMatToaster matToaster { get; set; }

        [Inject] protected IStringLocalizer<Global> L { get; set; }

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
                var reload = false;

                if (settings.ContainsKey(SettingKey.MainConfiguration_Runtime) && settings[SettingKey.MainConfiguration_Runtime].EntityAspect.HasChanges())
                    reload = true;

                await apiClient.SaveChanges();

                matToaster.Add(L["Operation Successful"], MatToastType.Success);

                if (reload)
                    navigationManager.NavigateTo(navigationManager.Uri, true);

            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }
    }
}
