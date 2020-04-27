using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.AspNetCore.Http;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IAccountManager
    {
        Task<ApiResponse> BuildLoginViewModel(string returnUrl);
        Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(ClaimsPrincipal userClaimsPrincipal, HttpContext httpContext, string logoutId);
        Task<ApiResponse> Login(LoginInputModel parameters);

        Task<ApiResponse> Register(RegisterDto parameters);

        Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters);

        Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters);

        Task<ApiResponse> ResetPassword(ResetPasswordDto parameters);

        Task<ApiResponse> Logout(ClaimsPrincipal userClaimsPrincipal);

        Task<ApiResponse> UserInfo(ClaimsPrincipal userClaimsPrincipal);

        Task<ApiResponse> UpdateUser(UserInfoDto userInfo);
        
        // Admin policies. 

        Task<ApiResponse> Create(RegisterDto parameters);

        Task<ApiResponse> Delete(string id);

        ApiResponse GetUser(ClaimsPrincipal userClaimsPrincipal);

        Task<ApiResponse> ListRoles();

        Task<ApiResponse> Update(UserInfoDto userInfo);

        Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, string newPassword, ClaimsPrincipal userClaimsPrincipal);
        
        Task<ApplicationUser> RegisterNewUserAsync(string userName, string email, string password, bool requireConfirmEmail);
    }
}
