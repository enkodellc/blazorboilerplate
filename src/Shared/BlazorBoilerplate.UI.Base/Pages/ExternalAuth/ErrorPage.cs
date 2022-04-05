using BlazorBoilerplate.Shared.Dto.ExternalAuth;
using Microsoft.AspNetCore.Components;

namespace BlazorBoilerplate.UI.Base.Pages.ExternalAuth
{
    public class ErrorPage : ComponentBase
    {
        [Parameter]
        public string ErrorEnumValue { get; set; }

        [Parameter]
        public string Description { get; set; }

        ErrorEnum error = ErrorEnum.Unknown;
        protected string errorText;
        protected override void OnInitialized()
        {
            var result = Enum.TryParse(ErrorEnumValue, out error);
            if (result == false)
                error = ErrorEnum.Unknown;

            errorText = error switch
            {
                ErrorEnum.UserCreationFailed => "User cannot be created",
                ErrorEnum.UserIsNotAllowed => "Login not allowed, check email inbox for account confirmation",
                ErrorEnum.UserLockedOut => "User is locked out",
                ErrorEnum.CannotAddExternalLogin => "Cannot create binding for this external login provider to the account",
                ErrorEnum.ExternalAuthError => "External provider cannot authenticate.\nCheck configuration.",
                ErrorEnum.ExternalUnknownUserId => "External authentication provider did not pass user identifier",
                ErrorEnum.ProviderNotFound => "Choosen provider has not been found/configured",
                ErrorEnum.Domain => string.Empty,
                ErrorEnum.Unknown => "Unknown reason",
                _ => "Unknown reason",
            };
        }
    }
}
