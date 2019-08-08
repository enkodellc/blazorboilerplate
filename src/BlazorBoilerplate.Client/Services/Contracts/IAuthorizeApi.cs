using BlazorBoilerplate.Shared;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Client.Services.Contracts
{
    public interface IAuthorizeApi
    {
        Task<ClientApiResponse> Login(LoginParameters loginParameters);
        Task<ClientApiResponse> Register(RegisterParameters registerParameters);
        Task<ClientApiResponse> ForgotPassword(ForgotPasswordParameters forgotPasswordParameters);
        Task<ClientApiResponse> ResetPassword(ResetPasswordParameters resetPasswordParameters);
        Task<ClientApiResponse> Logout();
        Task<ClientApiResponse> ConfirmEmail(ConfirmEmailParameters confirmEmailParameters);
        Task<UserInfo> GetUserInfo();
        Task<ClientApiResponse> UpdateUser(UserInfo userInfo);
    }
}
