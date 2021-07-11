using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Karambolo.Common.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using ObjectCloner.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static BlazorBoilerplate.Shared.Localizer.Settings;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public abstract class TranslationsPage : ComponentBase
    {
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] ILocalizationApiClient localizationApiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        protected List<LocalizationRecordKey> localizationRecordKeys { get; set; }

        protected Dictionary<string, PluralFormRule> PluralFormRules { get; set; }

        protected List<string> LocalizationCultures { get; set; } = new List<string>();

        protected List<LocalizationRecord> localizationRecords { get; set; } = new List<LocalizationRecord>();
        protected int pageSize { get; set; } = 10;
        private int pageIndex { get; set; } = 0;
        protected int totalItemsCount { get; set; } = 0;

        protected bool isDeleteDialogOpen = false;
        protected bool isEditDialogOpen = false;
        protected bool isNewKeyDialogOpen = false;
        protected bool isPluralDialogOpen = false;
        protected bool isEditAsHtmlDialogOpen = false;

        protected LocalizationRecordKey currentKey { get; set; }
        protected LocalizationRecordKey newKey { get; set; } = new LocalizationRecordKey();

        private LocalizationRecordKey oldKey { get; set; }

        protected LocalizationRecord newLocalizationRecord { get; set; } = new LocalizationRecord();

        protected LocalizationRecord currentLocalizationRecord { get; set; } = new LocalizationRecord();

        protected PluralTranslation newPlural { get; set; } = new PluralTranslation();

        protected override async Task OnInitializedAsync()
        {
            await LoadKeys();
            await LoadPluralFormRules();
        }

        protected async Task OnPage(int index, int size)
        {
            pageSize = size;
            pageIndex = index;

            await LoadKeys();
        }
        protected async Task LoadKeys(string filter = null)
        {
            localizationRecords = new List<LocalizationRecord>();

            try
            {
                localizationApiClient.ClearEntitiesCache();
                var result = await localizationApiClient.GetLocalizationRecordKeys(pageSize, pageIndex * pageSize, filter);
                localizationRecordKeys = new List<LocalizationRecordKey>(result);
                totalItemsCount = (int)result.InlineCount.Value;

                viewNotifier.Show(L["One item found", Plural.From("{0} items found", totalItemsCount)], ViewNotifierType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        private async Task LoadPluralFormRules()
        {
            try
            {
                var result = await localizationApiClient.GetPluralFormRules();
                PluralFormRules = result.ToDictionary(i => i.Language);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task LoadLocalizationRecords(LocalizationRecordKey key)
        {
            if (key != null)
                try
                {
                    currentKey = key;
                    localizationApiClient.ClearEntitiesCache();
                    var result = await localizationApiClient.GetLocalizationRecords(key);
                    localizationRecords = new List<LocalizationRecord>(result);

                    LocalizationCultures.Clear();

                    LocalizationCultures.AddRange(SupportedCultures
                        .Where(i => !localizationRecords.Any(l => l.Culture == i)));

                    if (LocalizationCultures.Count > 0)
                        newLocalizationRecord = new LocalizationRecord() { ContextId = currentKey.ContextId, MsgId = currentKey.MsgId, Culture = LocalizationCultures[0] };

                    viewNotifier.Show(L["One item found", Plural.From("{0} items found", result.Count())], ViewNotifierType.Success, L["Operation Successful"]);
                }
                catch (Exception ex)
                {
                    viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
                }
            else
                localizationRecords = new List<LocalizationRecord>();

            StateHasChanged();
        }

        protected void OpenEditDialog(LocalizationRecordKey key)
        {
            newKey = key;
            oldKey = key.DeepClone();
            isEditDialogOpen = true;
        }

        protected async Task EditLocalizationRecordKey()
        {
            if (oldKey != null && newKey != null && oldKey != newKey)
                try
                {
                    localizationApiClient.ClearEntitiesCache();
                    var response = await localizationApiClient.EditLocalizationRecordKey(oldKey, newKey);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadKeys();
                        localizationRecords = new List<LocalizationRecord>();
                        LocalizationCultures.Clear();

                        viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    }
                    else
                        viewNotifier.Show(response.Message, ViewNotifierType.Warning, L["Operation Failed"]);
                }
                catch (Exception ex)
                {
                    viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
                }

            isEditDialogOpen = false;

            StateHasChanged();
        }

        protected async Task DeleteLocalizationRecordKey(LocalizationRecordKey key)
        {
            if (key != null)
                try
                {
                    localizationApiClient.ClearEntitiesCache();
                    var response = await localizationApiClient.DeleteLocalizationRecordKey(key);


                    if (response.IsSuccessStatusCode)
                    {
                        await LoadKeys();
                        localizationRecords = new List<LocalizationRecord>();
                        LocalizationCultures.Clear();

                        viewNotifier.Show(response.Message, ViewNotifierType.Success, L["Operation Successful"]);
                    }
                    else
                        viewNotifier.Show(response.Message, ViewNotifierType.Warning, L["Operation Failed"]);
                }
                catch (Exception ex)
                {
                    viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
                }

            isDeleteDialogOpen = false;

            StateHasChanged();
        }

        protected void OpenDeleteDialog(LocalizationRecordKey key)
        {
            currentKey = key;
            isDeleteDialogOpen = true;
        }

        protected void OpenPluralDialog(LocalizationRecord record)
        {
            currentLocalizationRecord = record;
            newPlural = new PluralTranslation();
            isPluralDialogOpen = true;
        }

        protected abstract void SetHTML(string html);
        protected abstract Task<string> GetHTML();
        protected void OpenEditAsHtmlDialog(LocalizationRecord record)
        {
            currentLocalizationRecord = record;
            SetHTML(currentLocalizationRecord.Translation);
            isEditAsHtmlDialogOpen = true;
        }
        protected void OpenNewKeyDialogOpen()
        {
            newLocalizationRecord = new LocalizationRecord();
            localizationApiClient.AddEntity(newLocalizationRecord);
            isNewKeyDialogOpen = true;
        }

        protected void DeleteLocalizationRecord(LocalizationRecord record)
        {
            localizationApiClient.RemoveEntity(record);
            localizationRecords.Remove(record);
        }

        protected void DeletePluralTranslation(PluralTranslation plural)
        {
            localizationApiClient.RemoveEntity(plural);
            currentLocalizationRecord.PluralTranslations.Remove(plural);
        }

        protected async Task<bool> SaveChanges()
        {
            var result = true;

            try
            {
                await localizationApiClient.SaveChanges();

                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
            }
            catch (Exception ex)
            {
                result = false;
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
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

        protected async Task SaveNewPlural()
        {
            newPlural.LocalizationRecord = currentLocalizationRecord;
            localizationApiClient.AddEntity(newPlural);

            if (currentLocalizationRecord.Culture == NeutralCulture
                && newPlural.Index == 1)
                foreach (var record in localizationRecords)
                    record.MsgIdPlural = newPlural.Translation;

            if (await SaveChanges())
                newPlural = new PluralTranslation();
        }

        protected async Task<bool> SavePluralChanges()
        {
            var msgIdPlural = localizationRecords.Single(i => i.Culture == NeutralCulture)
                .PluralTranslations.Single(i => i.Index == 1).Translation;

            foreach (var record in localizationRecords)
                record.MsgIdPlural = msgIdPlural;

            return await SaveChanges();
        }

        protected async Task SaveNewLocalizationRecord()
        {
            localizationApiClient.AddEntity(newLocalizationRecord);

            if (await SaveChanges())
                await LoadLocalizationRecords(currentKey);
        }

        protected async Task SaveLocalizationRecordAsHTML()
        {
            currentLocalizationRecord.Translation = await GetHTML();

            isEditAsHtmlDialogOpen = false;
        }

        protected async Task CancelChanges()
        {
            localizationApiClient.CancelChanges();
            isNewKeyDialogOpen = false;
            await LoadLocalizationRecords(currentKey);
        }

        protected void CancelPluralChanges()
        {
            localizationApiClient.CancelChanges();
            isPluralDialogOpen = false;
        }

        protected async Task ReloadTranslations()
        {
            try
            {
                var response = await localizationApiClient.ReloadTranslations();

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    navigationManager.NavigateTo(navigationManager.Uri, true);
                }
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Warning, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task Upload(IEnumerable<IFileUploadEntry> files)
        {
            foreach (var file in files)
            {
                if (Path.GetExtension(file.Name).ToLower() != ".po")
                    viewNotifier.Show(L["Only PO files"], ViewNotifierType.Warning, L["Operation Failed"]);
                else
                {
                    try
                    {
                        using var ms = new MemoryStream();
                        await file.WriteToStreamAsync(ms);

                        var content = new MultipartFormDataContent {
                            { new ByteArrayContent(ms.GetBuffer()), "\"uploadedFile\"", file.Name }
                        };

                        var response = await localizationApiClient.Upload(content);

                        if (response.IsSuccessStatusCode)
                        {
                            viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                            navigationManager.NavigateTo(navigationManager.Uri, true);
                        }
                        else
                            viewNotifier.Show(response.Message, ViewNotifierType.Warning, L["Operation Failed"]);
                    }
                    catch (Exception ex)
                    {
                        viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
                    }
                }
            }
        }
    }
}
