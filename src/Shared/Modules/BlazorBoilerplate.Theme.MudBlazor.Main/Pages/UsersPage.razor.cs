using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Theme.Material.Shared.Components;
using Microsoft.AspNetCore.Authorization;

namespace BlazorBoilerplate.Theme.Material.Main.Pages
{
    public class UsersBasePage : ItemsTableBase<Person>
    {
        protected bool isOperator;
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();

            var user = (await authenticationStateTask).User;
            isOperator = (await authorizationService.AuthorizeAsync(user, Policies.For(UserFeatures.Operator))).Succeeded;

            orderByDefaultField = "LastName";
        }
    }
}
