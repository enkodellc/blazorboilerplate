using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorBoilerplate.Shared.Services
{
    public class AccountApiClient : IAccountApiClient
    {
        private readonly ILogger<AccountApiClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public AccountApiClient(NavigationManager navigationManager, HttpClient httpClient, IJSRuntime jsRuntime, ILogger<AccountApiClient> logger)
        {
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task<ApiResponseDto<LoginViewModel>> BuildLoginViewModel(string returnUrl)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<LoginViewModel>>("api/Account/BuildLoginViewModel", returnUrl);
        }

        public async Task<ApiResponseDto<LogoutViewModel>> BuildLogoutViewModel(string logoutId)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<LogoutViewModel>>("api/Account/BuildLogoutViewModel", logoutId);
        }

        private async Task SubmitServerForm(string path, AccountFormModel model)
        {
            model.__RequestVerificationToken = await _jsRuntime.InvokeAsync<string>("interop.getElementByName", "__RequestVerificationToken");

            await _jsRuntime.InvokeAsync<string>("interop.submitForm", path, model);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Login(LoginInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto<LoginResponseModel>>("api/Account/Login", parameters);

            if (AppState.Runtime == BlazorRuntime.Server)
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/login/", parameters);

            return response;
        }

        public async Task<ApiResponseDto> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/LoginWith2fa", parameters);

            if (AppState.Runtime == BlazorRuntime.Server)
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/loginwith2fa/", parameters);

            return response;
        }
        public async Task<ApiResponseDto> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/LoginWithRecoveryCode", parameters);

            if (AppState.Runtime == BlazorRuntime.Server)
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/loginwith2fa/", parameters);

            return response;
        }

        public async Task<ApiResponseDto<LoggedOutViewModel>> Logout(LogoutViewModel logoutViewModel)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto<LoggedOutViewModel>>("api/Account/Logout", logoutViewModel);

            if (response.IsSuccessStatusCode)
            {
                if (AppState.Runtime == BlazorRuntime.WebAssembly)
                {
                    if (!string.IsNullOrEmpty(logoutViewModel?.ReturnUrl))
                        _navigationManager.NavigateTo(logoutViewModel.ReturnUrl, true);
                }
                else
                    await SubmitServerForm("/server/logout/", logoutViewModel ?? new AccountFormModel());
            }

            return response;
        }

        public async Task<ApiResponseDto> Create(RegisterViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Create", parameters);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Register(RegisterViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<LoginResponseModel>>("api/Account/Register", parameters);
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ConfirmEmail", parameters);
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ResetPassword", parameters);
        }

        public async Task<ApiResponseDto> UpdatePassword(UpdatePasswordViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdatePassword", parameters);
        }
        public async Task<ApiResponseDto> AdminChangePassword(ChangePasswordViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>($"api/Account/AdminUserPasswordReset", parameters);
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ForgotPassword", parameters);
        }

        public async Task<ApiResponseDto<UserViewModel>> EnableAuthenticator(AuthenticatorVerificationCodeViewModel parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/EnableAuthenticator", parameters);
        }
        public async Task<ApiResponseDto<UserViewModel>> DisableAuthenticator()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/DisableAuthenticator", null);
        }
        public async Task<ApiResponseDto<UserViewModel>> ForgetTwoFactorClient()
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/ForgetTwoFactorClient", null);

            if (AppState.Runtime == BlazorRuntime.Server)
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/ForgetTwoFactorClient/", new AccountFormModel());

            return response;
        }
        public async Task<ApiResponseDto<UserViewModel>> Enable2fa(string userId = null)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/Enable2fa", userId);
        }
        public async Task<ApiResponseDto<UserViewModel>> Disable2fa(string userId = null)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/Disable2fa", userId);
        }

        public async Task<UserViewModel> GetUserViewModel()
        {
            UserViewModel userViewModel = new() { IsAuthenticated = false, Roles = new List<string>() };

            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/UserViewModel");

            if (apiResponse.IsSuccessStatusCode)
                userViewModel = apiResponse.Result;

            return userViewModel;
        }

        public async Task<ApiResponseDto<UserViewModel>> GetUserViewModel(string id)
        {
            return await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserViewModel>>($"api/Account/UserViewModel/{id}");
        }

        public async Task<UserViewModel> GetUser()
        {
            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/GetUser");
            return apiResponse.Result;
        }

        public async Task<ApiResponseDto> UpdateUser(UserViewModel userViewModel)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdateUser", userViewModel);
        }

        public async Task<ApiResponseDto> UpsertUser(UserViewModel userViewModel)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpsertUser", userViewModel);
        }
        public async Task<ApiResponseDto> DeleteUser(string id)
        {
            return await _httpClient.DeleteAsync<ApiResponseDto>($"api/Account/{id}");
        }

        public async Task<ApiResponseDto> DeleteMe()
        {
            return await _httpClient.DeleteAsync<ApiResponseDto>("api/Account");
        }

        public async Task<ApiResponseDto> AdminUpdateUser(UserViewModel userViewModel)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/AdminUpdateUser", userViewModel);
        }
    }
}
