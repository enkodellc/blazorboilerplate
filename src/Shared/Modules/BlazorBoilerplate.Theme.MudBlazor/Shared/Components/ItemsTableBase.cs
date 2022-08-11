using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Humanizer;
using MudBlazor;

namespace BlazorBoilerplate.Theme.Material.Shared.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
    public class ItemsTableBase<T> : BaseComponent
    {
        protected List<T> items;
        protected HashSet<T> selected = new(); 
        protected int pageSize;
        protected int pageIndex;
        protected int totalItemsCount;
        protected bool isBusy = true;
        protected string filter;
        protected QueryParameters queryParameters;
        protected string orderByDefaultField;
        protected string orderBy;
        protected string orderByDescending;
        protected bool waitingForFilter = false;

        protected async Task OnPage(int index, int size)
        {
            pageSize = size;
            pageIndex = index;

            await LoadItems();
        }

        protected virtual async Task LoadItems()
        {
            try
            {
                isBusy = true;

                await InvokeAsync(StateHasChanged);

                apiClient.ClearEntitiesCache();

                var from = $"{typeof(T).Name}".Pluralize(true);

                var prop = queryParameters?.GetType().GetProperties().SingleOrDefault(i => i.Name == "Filter");

                prop?.SetValue(queryParameters, filter);


                var result = await apiClient.GetItemsByFilter<T>(from, orderByDefaultField, filter, orderBy, orderByDescending, pageSize, pageIndex * pageSize, queryParameters?.ToDictionary());

                items = new List<T>(result);
                totalItemsCount = (int)result.InlineCount.Value;


                if (totalItemsCount == 0 && filter == null)
                    viewNotifier.Show(L["Data not available."], ViewNotifierType.Info);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                isBusy = false;

                await InvokeAsync(StateHasChanged);
            }
        }

        protected MudTable<T> table;

        protected async Task<TableData<T>> ServerReload(TableState state)
        {
            if (waitingForFilter) //use if you want to wait till your filter is loaded before filling the data
            {
                waitingForFilter = false;
                return new TableData<T>() { TotalItems = 0, Items = new List<T>() };
            }
            
            switch (state.SortDirection)
            {
                case SortDirection.Ascending:
                    if (orderBy != state.SortLabel)
                        table.CurrentPage = 0;

                    orderBy = state.SortLabel;
                    orderByDescending = null;
                    break;
                case SortDirection.Descending:
                    if (orderByDescending != state.SortLabel)
                        table.CurrentPage = 0;

                    orderByDescending = state.SortLabel;
                    orderBy = null;
                    break;
                case SortDirection.None:
                    if (orderBy != null || orderByDescending != null)
                        table.CurrentPage = 0;

                    orderBy = null;
                    orderByDescending = null;
                    break;
            }

            await OnPage(state.Page, state.PageSize);

            return new TableData<T>() { TotalItems = totalItemsCount, Items = items };
        }

        protected virtual async Task OnSearch(string text)
        {
            filter = text;
            await Reload();
        }

        protected async Task Reload()
        {
            table.CurrentPage = 0;
            await table.ReloadServerData();
        }
    }
}
