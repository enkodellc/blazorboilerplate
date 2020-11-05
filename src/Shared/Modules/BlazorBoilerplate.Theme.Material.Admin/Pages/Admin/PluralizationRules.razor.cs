using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Karambolo.Common.Localization;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin
{
    public class PluralizationRulesPage : ComponentBase
    {
        [Inject] IMatToaster matToaster { get; set; }
        [Inject] ILocalizationApiClient localizationApiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        protected List<PluralFormRule> PluralFormRules { get; set; }
        protected List<string> LocalizationCultures { get; set; } = new List<string>();
        protected PluralFormRule currentPluralFormRule { get; set; } = new PluralFormRule();
        protected PluralFormRule newPluralFormRule { get; set; } = new PluralFormRule();

        protected override async Task OnInitializedAsync()
        {
            await LoadPluralFormRules();
        }
        protected async Task LoadPluralFormRules()
        {
            PluralFormRules = new List<PluralFormRule>();

            try
            {
                localizationApiClient.ClearEntitiesCache();
                var result = await localizationApiClient.GetPluralFormRules();
                PluralFormRules = new List<PluralFormRule>(result);

                LocalizationCultures.Clear();

                LocalizationCultures.AddRange(BlazorBoilerplate.Shared.Localizer.Settings.SupportedCultures
                    .Where(i => !PluralFormRules.Any(l => l.Language == i)));

                matToaster.Add(L["One item found", Plural.From("{0} items found", PluralFormRules.Count)], MatToastType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }
        protected async Task OpenDelete(PluralFormRule pluralFormRule)
        {
            localizationApiClient.RemoveEntity(pluralFormRule);

            if (await SaveChanges())
            {
                newPluralFormRule = new PluralFormRule();
                await LoadPluralFormRules();
            }
        }
        protected async Task<bool> SaveChanges()
        {
            var result = true;

            try
            {
                await localizationApiClient.SaveChanges();

                matToaster.Add(L["Operation Successful"], MatToastType.Success);
            }
            catch (Exception ex)
            {
                result = false;
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }

            return result;
        }

        protected void CancelChanges()
        {
            localizationApiClient.CancelChanges();
        }

        protected async Task SaveNewPluralFormRule()
        {
            localizationApiClient.AddEntity(newPluralFormRule);

            if (await SaveChanges())
            {
                newPluralFormRule = new PluralFormRule();
                await LoadPluralFormRules();
            }
        }
    }
}
