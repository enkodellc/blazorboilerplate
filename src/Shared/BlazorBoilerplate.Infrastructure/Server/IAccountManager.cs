using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Models.Account;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IAccountManager
    {
        Task<ApiResponse> BuildLoginViewModel(string returnUrl);
        Task<LoggedOutViewModel> BuildLoggedOutViewModel(ClaimsPrincipal authenticatedUser, HttpContext httpContext, string logoutId);

        Task<ApiResponse> Login(LoginInputModel parameters);
        Task<ApiResponse> LoginWith2fa(LoginWith2faInputModel parameters);
        Task<ApiResponse> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters);
        Task<ApiResponse> Logout(ClaimsPrincipal authenticatedUser);

        Task<ApiResponse> Register(RegisterViewModel parameters);

        Task<ApiResponse> ConfirmEmail(ConfirmEmailViewModel parameters);

        Task<ApiResponse> ForgotPassword(ForgotPasswordViewModel parameters);
        Task<ApiResponse> ResetPassword(ResetPasswordViewModel parameters);
        Task<ApiResponse> UpdatePassword(ClaimsPrincipal authenticatedUser, UpdatePasswordViewModel parameters);

        Task<ApiResponse> EnableAuthenticator(ClaimsPrincipal authenticatedUser, AuthenticatorVerificationCodeViewModel parameters);
        Task<ApiResponse> DisableAuthenticator(ClaimsPrincipal authenticatedUser);
        Task<ApiResponse> ForgetTwoFactorClient(ClaimsPrincipal authenticatedUser);
        Task<ApiResponse> Enable2fa(Guid userId, ClaimsPrincipal authenticatedUser = null);
        Task<ApiResponse> Disable2fa(Guid userId, ClaimsPrincipal authenticatedUser = null);

        Task<ApiResponse> UserViewModel(ClaimsPrincipal authenticatedUser);
        Task<ApiResponse> UserViewModel(Guid id);

        Task<ApiResponse> UpdateUser(UserViewModel userViewModel, bool isUpsert, ClaimsPrincipal authenticatedUser);

        // Admin policies. 

        Task<ApiResponse> Create(RegisterViewModel parameters);

        Task<ApplicationUser> RegisterNewUser(string userName, string email, string password, bool emailConfirmedByAdmin);

        Task<ApiResponse> Delete(string id);

        ApiResponse GetUser(ClaimsPrincipal authenticatedUser);

        Task<ApiResponse> AdminUpdateUser(UserViewModel userViewModel);

        Task<ApiResponse> AdminResetUserPassword(ChangePasswordViewModel changePasswordViewModel, ClaimsPrincipal authenticatedUser);
    }
}
