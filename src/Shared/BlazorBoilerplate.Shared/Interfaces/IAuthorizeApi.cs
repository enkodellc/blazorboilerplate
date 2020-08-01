using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Account;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IAuthorizeApi
    {
        Task<ApiResponseDto<LoginViewModel>> BuildLoginViewModel(string returnUrl);
        Task<ApiResponseDto<LoginResponseModel>> Login(LoginInputModel parameters);
        Task<ApiResponseDto> LoginWith2fa(LoginWith2faInputModel parameters);
        Task<ApiResponseDto> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters);
        Task<ApiResponseDto> Create(RegisterDto parameters);
        Task<ApiResponseDto<LoginResponseModel>> Register(RegisterDto parameters);
        Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto parameters);
        Task<ApiResponseDto> ResetPassword(ResetPasswordDto parameters);
        Task<ApiResponseDto> UpdatePassword(UpdatePasswordDto parameters);
        Task<ApiResponseDto> Logout();
        Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto parameters);
        Task<UserInfo> GetUserInfo();
        Task<ApiResponseDto> UpdateUser(UserInfo userInfo);
        Task<UserInfo> GetUser();

        Task<ApiResponseDto<UserInfo>> EnableAuthenticator(AuthenticatorVerificationCodeDto parameters);
        Task<ApiResponseDto<UserInfo>> DisableAuthenticator();
        Task<ApiResponseDto<UserInfo>> ForgetTwoFactorClient();
        Task<ApiResponseDto<UserInfo>> Enable2fa();
        Task<ApiResponseDto<UserInfo>> Disable2fa();
    }
}
