using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Karambolo.Common.Localization;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin
{
    public class TranslationsPage : ComponentBase
    {
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] IMatToaster matToaster { get; set; }
        [Inject] ILocalizationApiClient localizationApiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        protected List<string> localizationRecordMsgIds { get; set; }

        protected List<string> LocalizationCultures { get; set; } = new List<string>();

        protected List<LocalizationRecord> localizationRecords { get; set; } = new List<LocalizationRecord>();
        protected int pageSize { get; set; } = 10;
        private int pageIndex { get; set; } = 0;
        protected int totalItemsCount { get; set; } = 0;

        protected bool isDeleteDialogOpen = false;
        protected bool isEditDialogOpen = false;
        protected bool isNewMsgIdDialogOpen = false;


        protected string currentMsgId { get; set; }
        protected string newMsgId { get; set; }

        protected LocalizationRecord newLocalizationRecord { get; set; } = new LocalizationRecord();

        protected override async Task OnInitializedAsync()
        {
            await LoadMsgIds();
        }

        protected async Task OnPage(MatPaginatorPageEvent e)
        {
            pageSize = e.PageSize;
            pageIndex = e.PageIndex;

            await LoadMsgIds();
        }
        protected async Task LoadMsgIds(string filter = null)
        {
            localizationRecords = new List<LocalizationRecord>();

            try
            {
                localizationApiClient.ClearEntitiesCache();
                var result = await localizationApiClient.GetLocalizationRecordMsgIds(pageSize, pageIndex * pageSize, filter);
                localizationRecordMsgIds = new List<string>(result);
                totalItemsCount = (int)result.InlineCount.Value;

                matToaster.Add(L["One item found", Plural.From("{0} items found", totalItemsCount)], MatToastType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        protected async Task LoadLocalizationRecords(string msgId)
        {
            if (msgId != null)
                try
                {
                    currentMsgId = msgId;
                    localizationApiClient.ClearEntitiesCache();
                    var result = await localizationApiClient.GetLocalizationRecords(msgId);
                    localizationRecords = new List<LocalizationRecord>(result);

                    LocalizationCultures.Clear();

                    LocalizationCultures.AddRange(BlazorBoilerplate.Shared.Localizer.Settings.SupportedCultures
                        .Where(i => !localizationRecords.Any(l => l.Culture == i)));

                    if (LocalizationCultures.Count > 0)
                        newLocalizationRecord = new LocalizationRecord() { MsgId = currentMsgId, Culture = LocalizationCultures[0] };

                    matToaster.Add(L["One item found", Plural.From("{0} items found", result.Count())], MatToastType.Success, L["Operation Successful"]);
                }
                catch (Exception ex)
                {
                    matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
                }
            else
                localizationRecords = new List<LocalizationRecord>();

            StateHasChanged();
        }

        protected void OpenEditDialog(string msgId)
        {
            currentMsgId = newMsgId = msgId;
            isEditDialogOpen = true;
        }

        protected async Task EditLocalizationRecordMsgId(string msgId)
        {
            if (msgId != null && currentMsgId != null)
                try
                {
                    localizationApiClient.ClearEntitiesCache();
                    var response = await localizationApiClient.EditLocalizationRecordMsgId(currentMsgId, msgId);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadMsgIds();
                        localizationRecords = new List<LocalizationRecord>();

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

        protected async Task DeleteLocalizationRecordMsgId(string msgId)
        {
            if (msgId != null)
                try
                {
                    localizationApiClient.ClearEntitiesCache();
                    var response = await localizationApiClient.DeleteLocalizationRecordMsgId(msgId);


                    if (response.IsSuccessStatusCode)
                    {
                        await LoadMsgIds();
                        localizationRecords = new List<LocalizationRecord>();

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

        protected void OpenDeleteDialog(string msgId)
        {
            currentMsgId = msgId;
            isDeleteDialogOpen = true;
        }

        protected void OpenNewMsgIdDialogOpen()
        {
            newLocalizationRecord = new LocalizationRecord();
            localizationApiClient.AddEntity(newLocalizationRecord);
            isNewMsgIdDialogOpen = true;
        }

        protected void DeleteLocalizationRecord(LocalizationRecord record)
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

        protected async Task SaveNewMsgId()
        {
            if (await SaveChanges())
            {
                isNewMsgIdDialogOpen = false;
                await LoadMsgIds();
                newLocalizationRecord = new LocalizationRecord();
            }
        }

        protected async Task SaveNewLocalizationRecord()
        {
            localizationApiClient.AddEntity(newLocalizationRecord);

            if (await SaveChanges())
                await LoadLocalizationRecords(currentMsgId);
        }

        protected async Task CancelChanges()
        {
            localizationApiClient.CancelChanges();
            isNewMsgIdDialogOpen = false;
            await LoadLocalizationRecords(currentMsgId);
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

        protected async Task Upload(IMatFileUploadEntry[] files)
        {
            foreach (var file in files)
            {
                if (Path.GetExtension(file.Name).ToLower() != ".po")
                    matToaster.Add(L["Only PO files"], MatToastType.Warning, L["Operation Failed"]);
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
    }
}
