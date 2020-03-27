using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IAccountManager
    {
        Task<ApiResponse> Login(LoginDto parameters);

        Task<ApiResponse> Register(RegisterDto parameters);

        Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters);

        Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters);

        Task<ApiResponse> ResetPassword(ResetPasswordDto parameters);

        Task<ApiResponse> Logout();

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
