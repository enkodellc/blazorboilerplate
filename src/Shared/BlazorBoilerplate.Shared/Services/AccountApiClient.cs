using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBoilerplate.Shared.Services
{
    public class AccountApiClient : IAccountApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public AccountApiClient(NavigationManager navigationManager, HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponseDto<LoginViewModel>> BuildLoginViewModel(string returnUrl)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<LoginViewModel>>("api/Account/BuildLoginViewModel", returnUrl);
        }

        private async Task SubmitServerForm(string path, AccountFormModel model)
        {
            model.__RequestVerificationToken = await _jsRuntime.InvokeAsync<string>("interop.getElementByName", "__RequestVerificationToken");

            await _jsRuntime.InvokeAsync<string>("interop.submitForm", path, model);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Login(LoginInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto<LoginResponseModel>>("api/Account/Login", parameters);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/login/", parameters);

            return response;
        }

        public async Task<ApiResponseDto> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/LoginWith2fa", parameters);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/loginwith2fa/", parameters);

            return response;
        }
        public async Task<ApiResponseDto> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/LoginWithRecoveryCode", parameters);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/loginwith2fa/", parameters);

            return response;
        }

        public async Task<ApiResponseDto> Logout(string returnUrl = null)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Logout", null);

            if (response.IsSuccessStatusCode)
            {
                var logoutModel = new AccountFormModel() { ReturnUrl = returnUrl };

                if (_navigationManager.IsWebAssembly())
                {
                    if (!string.IsNullOrEmpty(logoutModel.ReturnUrl))
                        _navigationManager.NavigateTo(logoutModel.ReturnUrl);
                }
                else
                    await SubmitServerForm("/server/logout/", logoutModel);
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

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/ForgetTwoFactorClient/", new AccountFormModel());

            return response;
        }
        public async Task<ApiResponseDto<UserViewModel>> Enable2fa()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/Enable2fa", null);
        }
        public async Task<ApiResponseDto<UserViewModel>> Disable2fa()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/Disable2fa", null);
        }

        public async Task<UserViewModel> GetUserViewModel()
        {
            UserViewModel userViewModel = new UserViewModel { IsAuthenticated = false, Roles = new List<string>() };

            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserViewModel>>("api/Account/UserViewModel");

            if (apiResponse.IsSuccessStatusCode)
                userViewModel = apiResponse.Result;

            return userViewModel;
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

        public async Task<ApiResponseDto> AdminUpdateUser(UserViewModel userViewModel)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/AdminUpdateUser", userViewModel);
        }
    }
}
