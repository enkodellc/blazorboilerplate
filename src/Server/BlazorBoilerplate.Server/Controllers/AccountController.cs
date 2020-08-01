using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NSwag.Annotations;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [OpenApiIgnore]
    [SecurityHeaders]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager _accountManager;

        private readonly ApiResponse _invalidData;

        private readonly IStringLocalizer<Strings> L;

        public AccountController(IAccountManager accountManager, IStringLocalizer<Strings> l)
        {
            _accountManager = accountManager;
            L = l;
            _invalidData = new ApiResponse(Status400BadRequest, L["InvalidData"]);
        }

        [HttpPost("BuildLoginViewModel")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> BuildLoginViewModel(string returnUrl)
            => await _accountManager.BuildLoginViewModel(returnUrl);

        // POST: api/Account/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse<LoginResponseModel>> Login(LoginInputModel parameters)
        {
            return ModelState.IsValid ? await _accountManager.Login(parameters) : new ApiResponse<LoginResponseModel>(Status400BadRequest, L["InvalidData"]);
        }

        // POST: api/Account/LoginWithRecoveryCode
        [HttpPost("LoginWithRecoveryCode")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            return ModelState.IsValid ? await _accountManager.LoginWithRecoveryCode(parameters) : _invalidData;
        }

        // POST: api/Account/LoginWith2fa
        [HttpPost("LoginWith2fa")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            return ModelState.IsValid ? await _accountManager.LoginWith2fa(parameters) : _invalidData;
        }

        // POST: api/Account/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<ApiResponse> Logout()
        {
            var response = await _accountManager.Logout(User);

            var vm = await _accountManager.BuildLoggedOutViewModelAsync(User, HttpContext, null);

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return response;
        }

        // POST: api/Account/Register
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ApiResponse<LoginResponseModel>> Register(RegisterDto parameters)
            => ModelState.IsValid ? await _accountManager.Register(parameters) : new ApiResponse<LoginResponseModel>(Status400BadRequest, L["InvalidData"]);

        // POST: api/Account/ConfirmEmail
        [HttpPost("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters)
            => ModelState.IsValid ? await _accountManager.ConfirmEmail(parameters) : _invalidData;

        // POST: api/Account/ForgotPassword
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters)
            => ModelState.IsValid ? await _accountManager.ForgotPassword(parameters) : _invalidData;

        //api/Account/ResetPassword
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ResetPassword(ResetPasswordDto parameters)
        => ModelState.IsValid ? await _accountManager.ResetPassword(parameters) : _invalidData;

        //api/Account/UpdatePassword
        [HttpPost("UpdatePassword")]
        public async Task<ApiResponse> UpdatePassword(UpdatePasswordDto parameters)
        => ModelState.IsValid ? await _accountManager.UpdatePassword(User, parameters) : _invalidData;

        [HttpPost("EnableAuthenticator")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> EnableAuthenticator(AuthenticatorVerificationCodeDto parameters)
        => ModelState.IsValid ? await _accountManager.EnableAuthenticator(User, parameters) : _invalidData;

        [HttpPost("DisableAuthenticator")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> DisableAuthenticator()
        => await _accountManager.DisableAuthenticator(User);

        [HttpPost("ForgetTwoFactorClient")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> ForgetTwoFactorClient()
        => await _accountManager.ForgetTwoFactorClient(User);

        [HttpPost("Enable2fa")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> Enable2fa()
        => await _accountManager.Enable2fa(User);

        [HttpPost("Disable2fa")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> Disable2fa()
        => await _accountManager.Disable2fa(User);

        [HttpGet("UserInfo")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> UserInfo()
        => await _accountManager.UserInfo(User);

        // DELETE: api/Account/5
        [HttpPost("UpdateUser")]
        [Authorize]
        public async Task<ApiResponse> UpdateUser(UserInfo userInfo)
        => ModelState.IsValid ? await _accountManager.UpdateUser(userInfo) : _invalidData;

        ///----------Admin User Management Interface Methods
        // POST: api/Account/Create
        [HttpPost("Create")]
        [Authorize(Permissions.User.Create)]
        public async Task<ApiResponse> Create(RegisterDto parameters)
        => ModelState.IsValid ? await _accountManager.Create(parameters) : _invalidData;

        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        [Authorize(Permissions.User.Delete)]
        public async Task<ApiResponse> Delete(string id)
        => await _accountManager.Delete(id);

        [HttpGet("GetUser")]
        public ApiResponse GetUser()
        => _accountManager.GetUser(User);

        [HttpGet("ListRoles")]
        [Authorize(Permissions.Role.Read)]
        public async Task<ApiResponse> ListRoles()
        => await _accountManager.ListRoles();

        [HttpPut]
        [Authorize(Permissions.User.Update)]
        // PUT: api/Account/5
        public async Task<ApiResponse> Update([FromBody] UserInfo userInfo)
        => ModelState.IsValid ? await _accountManager.Update(userInfo) : _invalidData;

        [HttpPost("AdminUserPasswordReset/{id}")]
        [Authorize(Permissions.User.Update)]
        [ProducesResponseType(Status204NoContent)]
        public async Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, [FromBody] string newPassword)
        => ModelState.IsValid
                ? await _accountManager.AdminResetUserPasswordAsync(id, newPassword, User)
                : _invalidData;
    }
}
