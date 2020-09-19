using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.SqlLocalizer;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin
{
    public class TranslationsPage : ComponentBase
    {
        [Inject] private NavigationManager navigationManager { get; set; }
        [Inject] IMatToaster matToaster { get; set; }
        [Inject] ILocalizationApiClient localizationApiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        protected List<string> localizationRecordKeys { get; set; }

        protected List<string> LocalizationCultures { get; set; } = new List<string>();

        protected List<ILocalizationRecord> localizationRecords { get; set; } = new List<ILocalizationRecord>();
        protected int pageSize { get; set; } = 10;
        private int pageIndex { get; set; } = 0;
        protected int totalItemsCount { get; set; } = 0;

        protected bool isDeleteDialogOpen = false;
        protected bool isEditDialogOpen = false;
        protected bool isNewKeyDialogOpen = false;


        protected string currentKey { get; set; }
        protected string newKey { get; set; }

        protected ILocalizationRecord newLocalizationRecord { get; set; } = new LocalizationRecord();

        protected override async Task OnInitializedAsync()
        {
            await LoadKeys();
        }

        protected async Task OnPage(MatPaginatorPageEvent e)
        {
            pageSize = e.PageSize;
            pageIndex = e.PageIndex;

            await LoadKeys();
        }
        protected async Task LoadKeys(string filter = null)
        {
            localizationRecords = new List<ILocalizationRecord>();

            try
            {
                localizationApiClient.ClearEntitiesCache();
                var result = await localizationApiClient.GetLocalizationRecordKeys(pageSize, pageIndex * pageSize, filter);
                localizationRecordKeys = new List<string>(result);
                totalItemsCount = (int)result.InlineCount.Value;

                matToaster.Add($"Total Items: {totalItemsCount}", MatToastType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        protected async Task LoadLocalizationRecords(string key)
        {
            if (key != null)
                try
                {
                    currentKey = key;
                    localizationApiClient.ClearEntitiesCache();
                    var result = await localizationApiClient.GetLocalizationRecords(key);
                    localizationRecords = new List<ILocalizationRecord>(result);

                    LocalizationCultures.Clear();

                    LocalizationCultures.AddRange(BlazorBoilerplate.Shared.SqlLocalizer.Settings.SupportedCultures
                        .Where(i => !localizationRecords.Any(l => l.LocalizationCulture == i)));

                    if (LocalizationCultures.Count > 0)
                        newLocalizationRecord = new LocalizationRecord() { Key = currentKey, LocalizationCulture = LocalizationCultures[0] };

                    matToaster.Add($"Total Items: {localizationRecords.Count}", MatToastType.Success, L["Operation Successful"]);
                }
                catch (Exception ex)
                {
                    matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
                }
            else
                localizationRecords = new List<ILocalizationRecord>();

            StateHasChanged();
        }

        protected void OpenEditDialog(string key)
        {
            currentKey = newKey = key;
            isEditDialogOpen = true;
        }

        protected async Task EditLocalizationRecordKey(string key)
        {
            if (key != null && currentKey != null)
                try
                {
                    localizationApiClient.ClearEntitiesCache();
                    var response = await localizationApiClient.EditLocalizationRecordKey(currentKey, key);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadKeys();
                        localizationRecords = new List<ILocalizationRecord>();

                        matToaster.Add(L["Operation Successful"], MatToastType.Success);
                    }
                    else
                        matToaster.Add(response.Message, MatToastType.Warning, L["Operation Failed"]);
                }
                catch (Exception ex)
                {
                    matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
                }

            isEditDialogOpen = false;

            StateHasChanged();
        }

        protected async Task DeleteLocalizationRecordKey(string key)
        {
            if (key != null)
                try
                {
                    localizationApiClient.ClearEntitiesCache();
                    var response = await localizationApiClient.DeleteLocalizationRecordKey(key);


                    if (response.IsSuccessStatusCode)
                    {
                        await LoadKeys();
                        localizationRecords = new List<ILocalizationRecord>();

                        matToaster.Add(response.Message, MatToastType.Success, L["Operation Successful"]);
                    }
                    else
                        matToaster.Add(response.Message, MatToastType.Warning, L["Operation Failed"]);
                }
                catch (Exception ex)
                {
                    matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
                }

            isDeleteDialogOpen = false;

            StateHasChanged();
        }

        protected void OpenDeleteDialog(string key)
        {
            currentKey = key;
            isDeleteDialogOpen = true;
        }

        protected void OpenNewKeyDialogOpen()
        {
            newLocalizationRecord = new LocalizationRecord();
            localizationApiClient.AddEntity(newLocalizationRecord);
            isNewKeyDialogOpen = true;
        }

        protected void DeleteLocalizationRecord(ILocalizationRecord record)
        {
            localizationApiClient.RemoveEntity(record);
            localizationRecords.Remove(record);
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

        protected async Task SaveNewKey()
        {
            if (await SaveChanges())
            {
                isNewKeyDialogOpen = false;
                await LoadKeys();
                newLocalizationRecord = new LocalizationRecord();
            }
        }

        protected async Task SaveNewLocalizationRecord()
        {
            localizationApiClient.AddEntity(newLocalizationRecord);

            if (await SaveChanges())
                await LoadLocalizationRecords(currentKey);
        }

        protected async Task CancelChanges()
        {
            localizationApiClient.CancelChanges();
            isNewKeyDialogOpen = false;
            await LoadLocalizationRecords(currentKey);
        }

        protected async Task ReloadTranslations()
        {
            try
            {
                var response = await localizationApiClient.ReloadTranslations();

                if (response.IsSuccessStatusCode)
                {
                    matToaster.Add(L["Operation Successful"], MatToastType.Success);
                    navigationManager.NavigateTo(navigationManager.Uri, true);
                }
                else
                    matToaster.Add(response.Message, MatToastType.Warning, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }        
        }
    }
}
