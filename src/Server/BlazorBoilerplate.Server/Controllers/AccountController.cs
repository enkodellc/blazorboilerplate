using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Account;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Controllers
{
    [SecurityHeaders]
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAccountManager _accountManager;
        private readonly ApiResponse _invalidData;

        private readonly IStringLocalizer<Global> L;

        public AccountController(
            IAuthorizationService authorizationService,
            IAccountManager accountManager,
            IStringLocalizer<Global> l)
        {
            _authorizationService = authorizationService;
            _accountManager = accountManager;
            L = l;
            _invalidData = new ApiResponse(Status400BadRequest, L["InvalidData"]);
        }

        [HttpPost("BuildLoginViewModel")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> BuildLoginViewModel([FromBody] string returnUrl)
            => await _accountManager.BuildLoginViewModel(returnUrl);

        [HttpPost("BuildLogoutViewModel")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> BuildLogoutViewModel([FromBody] string logoutId)
            => new ApiResponse(Status200OK, null, await _accountManager.BuildLogoutViewModel(User, logoutId));

        // POST: api/Account/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> Login(LoginInputModel parameters)
        {
            return ModelState.IsValid ? await _accountManager.Login(parameters) : new ApiResponse(Status400BadRequest, L["InvalidData"]);
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<ApiResponse> Logout([FromQuery] string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await _accountManager.BuildLogoutViewModel(User, logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return new ApiResponse(Status200OK);
        }

        // POST: api/Account/Logout
        [HttpPost("Logout")]
        [AllowAnonymous]
        public async Task<ApiResponse> Logout([FromBody] LogoutInputModel model)
        {
            var vm = await _accountManager.BuildLoggedOutViewModel(User, HttpContext, model?.LogoutId);

            var response = await _accountManager.Logout(User);

            response.Result = vm;

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action(nameof(Logout), new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return response;
        }

        // POST: api/Account/Register
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ApiResponse> Register(RegisterViewModel parameters)
            => ModelState.IsValid ? await _accountManager.Register(parameters) : new ApiResponse(Status400BadRequest, L["InvalidData"]);

        // POST: api/Account/ConfirmEmail
        [HttpPost("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailViewModel parameters)
            => ModelState.IsValid ? await _accountManager.ConfirmEmail(parameters) : _invalidData;

        // POST: api/Account/ForgotPassword
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ForgotPassword(ForgotPasswordViewModel parameters)
            => ModelState.IsValid ? await _accountManager.ForgotPassword(parameters) : _invalidData;

        //api/Account/ResetPassword
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ResetPassword(ResetPasswordViewModel parameters)
        => ModelState.IsValid ? await _accountManager.ResetPassword(parameters) : _invalidData;

        //api/Account/UpdatePassword
        [HttpPost("UpdatePassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> UpdatePassword(UpdatePasswordViewModel parameters)
        => ModelState.IsValid ? await _accountManager.UpdatePassword(User, parameters) : _invalidData;

        [HttpPost("EnableAuthenticator")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> EnableAuthenticator(AuthenticatorVerificationCodeViewModel parameters)
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
        {
            return await _accountManager.ForgetTwoFactorClient(User);
        }

        [HttpPost("Enable2fa")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> Enable2fa([FromBody] string userId)
        {
            if (userId != null)
            {
                if ((await _authorizationService.AuthorizeAsync(User, Policies.For(UserFeatures.Operator))).Succeeded)
                {

                    return await _accountManager.Enable2fa(GuidUtil.FromCompressedString(userId));
                }
                else
                    return new ApiResponse(Status401Unauthorized);
            }
            else
                return await _accountManager.Enable2fa(User.GetUserId(), User);
        }

        [HttpPost("Disable2fa")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> Disable2fa([FromBody] string userId)
        {
            if (userId != null)
            {
                if ((await _authorizationService.AuthorizeAsync(User, Policies.For(UserFeatures.Operator))).Succeeded)
                {

                    return await _accountManager.Disable2fa(GuidUtil.FromCompressedString(userId));
                }
                else
                    return new ApiResponse(Status401Unauthorized);
            }
            else
                return await _accountManager.Disable2fa(User.GetUserId(), User);
        }

        [HttpGet("UserViewModel/{id?}")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> UserViewModel(string id)
        {
            if (id != null)
            {
                if ((await _authorizationService.AuthorizeAsync(User, Policies.For(UserFeatures.Operator))).Succeeded)
                {
                    return await _accountManager.UserViewModel(GuidUtil.FromCompressedString(id));
                }
                else
                    return new ApiResponse(Status401Unauthorized);
            }
            else
                return await _accountManager.UserViewModel(User);
        }

        [HttpPost("UpdateUser")]
        public async Task<ApiResponse> UpdateUser(UserViewModel userViewModel)
        => ModelState.IsValid ? await _accountManager.UpdateUser(userViewModel, false, User) : _invalidData;

        [HttpPost("UpsertUser")]
        [AuthorizeForFeature(UserFeatures.Operator)]
        public async Task<ApiResponse> UpsertUser(UserViewModel userViewModel)
        => ModelState.IsValid ? await _accountManager.UpdateUser(userViewModel, true, User) : _invalidData;

        ///----------Admin User Management Interface Methods
        // POST: api/Account/Create
        [HttpPost("Create")]
        [Authorize(Permissions.User.Create)]
        public async Task<ApiResponse> Create(RegisterViewModel parameters)
        => ModelState.IsValid ? await _accountManager.Create(parameters) : _invalidData;

        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        [AuthorizeForFeature(UserFeatures.Operator)]
        public async Task<ApiResponse> Delete(string id)
        {
            return await _accountManager.Delete(id);
        }

        [HttpDelete]
        public async Task<ApiResponse> Delete()
        => await _accountManager.Delete(User.Identity.GetSubjectId());

        [HttpGet("GetUser")]
        [AllowAnonymous]
        public ApiResponse GetUser()
        => _accountManager.GetUser(User);

        [HttpPost("AdminUpdateUser")]
        [Authorize(Permissions.User.Update)]
        public async Task<ApiResponse> AdminUpdateUser([FromBody] UserViewModel userViewModel)
        => ModelState.IsValid ? await _accountManager.AdminUpdateUser(userViewModel) : _invalidData;

        [HttpPost("AdminUserPasswordReset")]
        [Authorize(Permissions.User.Update)]
        [ProducesResponseType(Status204NoContent)]
        public async Task<ApiResponse> AdminResetUserPasswordAsync(ChangePasswordViewModel changePasswordViewModel)
        => ModelState.IsValid ? await _accountManager.AdminResetUserPassword(changePasswordViewModel, User) : _invalidData;
    }
}

