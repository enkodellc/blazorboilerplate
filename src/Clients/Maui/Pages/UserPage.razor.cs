using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorBoilerplateMaui.Pages
{
    public class UserBasePage : BaseComponent
    {
        [Inject] protected IAccountApiClient accountApiClient { get; set; }
        [Parameter] public string id { get; set; }

        protected bool found = true;
        protected bool isEdit;
        protected UserViewModel model;

        protected override async Task OnInitializedAsync()
        {
            isEdit = navigationManager.Uri.ToLower().Contains("/edit/");

            if (isEdit)
            {
                if (id != null)
                {
                    var response = await accountApiClient.GetUserViewModel(id);

                    if (response.IsSuccessStatusCode)
                    {
                        model = response.Result;
                    }
                    else
                        found = false;
                }
                else
                    found = false;
            }
            else
            {
                var currentUser = await accountApiClient.GetUserViewModel();

                model = new UserViewModel() { UserNameEqualsEmail = true, ExpirationDate = currentUser.ExpirationDate ?? DateTime.Now.AddYears(1) };
            }
        }
    }
}