using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Karambolo.Common.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Linq.Expressions;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class DbLogViewerPage : ComponentBase
    {
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] IApiClient apiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        protected List<DbLog> dbLogItems;
        protected string[] DebugLevels = { "Debug", "Information", "Warning", "Error" };
        protected string DebugLevel = string.Empty;
        protected int pageSize { get; set; } = 10;
        protected int pageIndex { get; set; } = 0;
        protected int totalItemsCount { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task OnPage(int index, int size)
        {
            pageSize = size;
            pageIndex = index;

            await LoadData(DebugLevel);
        }

        protected async Task LoadData(string debugLevel = "")
        {
            try
            {
                DebugLevel = debugLevel;
                Expression<Func<DbLog, bool>> predicate = null;

                if (!string.IsNullOrWhiteSpace(debugLevel))
                    predicate = i => i.Level == debugLevel;

                var result = await apiClient.GetLogs(predicate, pageSize, pageIndex * pageSize);

                dbLogItems = new List<DbLog>(result);
                totalItemsCount = (int)result.InlineCount.Value;
                viewNotifier.Show(L["One item found", Plural.From("{0} items found", totalItemsCount)], ViewNotifierType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}
