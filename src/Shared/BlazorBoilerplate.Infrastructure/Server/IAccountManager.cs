using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Models.Account;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IAccountManager
    {
        Task<ApiResponse> BuildLoginViewModel(string returnUrl);
        Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(ClaimsPrincipal user, HttpContext httpContext, string logoutId);

        Task<ApiResponse> Login(LoginInputModel parameters);
        Task<ApiResponse> LoginWith2fa(LoginWith2faInputModel parameters);
        Task<ApiResponse> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters);
        Task<ApiResponse> Logout(ClaimsPrincipal user);

        Task<ApiResponse> Register(RegisterViewModel parameters);

        Task<ApiResponse> ConfirmEmail(ConfirmEmailViewModel parameters);

        Task<ApiResponse> ForgotPassword(ForgotPasswordViewModel parameters);
        Task<ApiResponse> ResetPassword(ResetPasswordViewModel parameters);
        Task<ApiResponse> UpdatePassword(ClaimsPrincipal user, UpdatePasswordViewModel parameters);

        Task<ApiResponse> EnableAuthenticator(ClaimsPrincipal user, AuthenticatorVerificationCodeViewModel parameters);
        Task<ApiResponse> DisableAuthenticator(ClaimsPrincipal user);
        Task<ApiResponse> ForgetTwoFactorClient(ClaimsPrincipal user);
        Task<ApiResponse> Enable2fa(ClaimsPrincipal user);
        Task<ApiResponse> Disable2fa(ClaimsPrincipal user);

        Task<ApiResponse> UserViewModel(ClaimsPrincipal user);

        Task<ApiResponse> UpdateUser(UserViewModel userViewModel);
        
        // Admin policies. 

        Task<ApiResponse> Create(RegisterViewModel parameters);

        Task<ApiResponse> Delete(string id);

        ApiResponse GetUser(ClaimsPrincipal user);

        Task<ApiResponse> ListRoles();

        Task<ApiResponse> Update(UserViewModel userViewModel);

        Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, string newPassword, ClaimsPrincipal user);
        
        Task<ApplicationUser> RegisterNewUserAsync(string userName, string email, string password, bool requireConfirmEmail);
    }
}
