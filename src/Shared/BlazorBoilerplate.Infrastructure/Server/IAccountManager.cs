using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Account;
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

        Task<ApiResponse> Register(RegisterDto parameters);

        Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters);

        Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters);
        Task<ApiResponse> ResetPassword(ResetPasswordDto parameters);
        Task<ApiResponse> UpdatePassword(ClaimsPrincipal user, UpdatePasswordDto parameters);

        Task<ApiResponse> EnableAuthenticator(ClaimsPrincipal user, AuthenticatorVerificationCodeDto parameters);
        Task<ApiResponse> DisableAuthenticator(ClaimsPrincipal user);
        Task<ApiResponse> ForgetTwoFactorClient(ClaimsPrincipal user);
        Task<ApiResponse> Enable2fa(ClaimsPrincipal user);
        Task<ApiResponse> Disable2fa(ClaimsPrincipal user);

        Task<ApiResponse> UserInfo(ClaimsPrincipal user);

        Task<ApiResponse> UpdateUser(UserInfo userInfo);
        
        // Admin policies. 

        Task<ApiResponse> Create(RegisterDto parameters);

        Task<ApiResponse> Delete(string id);

        ApiResponse GetUser(ClaimsPrincipal user);

        Task<ApiResponse> ListRoles();

        Task<ApiResponse> Update(UserInfo userInfo);

        Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, string newPassword, ClaimsPrincipal user);
        
        Task<ApplicationUser> RegisterNewUserAsync(string userName, string email, string password, bool requireConfirmEmail);
    }
}
