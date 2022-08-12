using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Theme.Material.Shared.Components;
using Breeze.Sharp;
using System.ComponentModel;

namespace BlazorBoilerplate.Theme.Material.Demo.Pages
{
    public class TodoPagingBasePage : ItemsTableBase<Todo>
    {
        protected ToDoFilter todoFilter = new();

        protected List<SelectItem<Guid?>> Creators = new();
        protected List<SelectItem<Guid?>> Editors = new();
        protected override async Task OnInitializedAsync()
        {
            //waitingForFilter = true; // set this if you want to wait to load your table data for whatever reason and set it to false when you want it to execute.
            await LoadFilters();

            queryParameters = todoFilter;

            todoFilter.PropertyChanged += FilterPropertyChanged;
        }

        private async void FilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            orderByDescending = "CreatedOn";

            isBusy = true;

            await LoadFilters(e.PropertyName);

            apiClient.ClearEntitiesCache();

            await Reload();

            isBusy = false;
        }

        private async Task LoadFilters(string propertyName = null)
        {
            var tasks = new Dictionary<string, Task>();

            if (propertyName != nameof(todoFilter.CreatedById))
                tasks.Add("GetTodoCreators", apiClient.GetTodoCreators(todoFilter));

            if (propertyName != nameof(todoFilter.ModifiedById))
                tasks.Add("GetTodoEditors", apiClient.GetTodoEditors(todoFilter));

            await Task.WhenAll(tasks.Values.ToArray());

            foreach (var task in tasks)
            {
                if (task.Key == "GetTodoCreators")
                {
                    var t = (Task<QueryResult<ApplicationUser>>)task.Value;

                    if (!t.IsFaulted)
                    {
                        Creators = t.Result.Select(i => new SelectItem<Guid?> { Id = i.Id, DisplayValue = i.UserName }).ToList();

                        Creators.Insert(0, new SelectItem<Guid?> { Id = null, DisplayValue = "-" });
                    }
                    else
                        viewNotifier.Show(t.Exception.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
                }
                else if (task.Key == "GetTodoEditors")
                {
                    var t = (Task<QueryResult<ApplicationUser>>)task.Value;

                    if (!t.IsFaulted)
                    {
                        Editors = t.Result.Select(i => new SelectItem<Guid?> { Id = i.Id, DisplayValue = i.UserName }).ToList();

                        Editors.Insert(0, new SelectItem<Guid?> { Id = null, DisplayValue = "-" });
                    }
                    else
                        viewNotifier.Show(t.Exception.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
                }
            }
        }

        protected override Task OnSearch(string text)
        {
            todoFilter.Query = text;

            return Task.CompletedTask;
        }

        protected async void Update(Todo todo)
        {
            try
            {
                todo.IsCompleted = !todo.IsCompleted;

                await apiClient.SaveChanges();

                viewNotifier.Show($"{todo.Title} updated", ViewNotifierType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        public override void Dispose()
        {
            todoFilter.PropertyChanged -= FilterPropertyChanged;

            base.Dispose();
        }
    }
}
