using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;
using BlazorBoilerplate.Infrastructure.Server;

namespace BlazorBoilerplate.Server.Controllers
{
    [SecurityHeaders]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager _accountManager;

        private readonly ApiResponse _invalidUserModel;

        private readonly IStringLocalizer<Strings> L;

        public AccountController(IAccountManager accountManager, IStringLocalizer<Strings> l)
        {
            _accountManager = accountManager;
            L = l;
            _invalidUserModel = new ApiResponse(Status400BadRequest, L["InvalidData"]); // Could we inject this? As some form of 'Errors which has constant values'?
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
        public async Task<ApiResponse> Login(LoginInputModel parameters)
        {
            return ModelState.IsValid ? await _accountManager.Login(parameters) : _invalidUserModel;
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
        public async Task<ApiResponse> Register(RegisterDto parameters)
            => ModelState.IsValid ? await _accountManager.Register(parameters) : _invalidUserModel;

        // POST: api/Account/ConfirmEmail
        [HttpPost("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters)
            => ModelState.IsValid ? await _accountManager.ConfirmEmail(parameters) : _invalidUserModel;

        // POST: api/Account/ForgotPassword
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters)
            => ModelState.IsValid ? await _accountManager.ForgotPassword(parameters) : _invalidUserModel;

        // PUT: api/Account/ResetPassword
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ResetPassword(ResetPasswordDto parameters)
        => ModelState.IsValid ? await _accountManager.ResetPassword(parameters) : _invalidUserModel;        

        [HttpGet("UserInfo")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        public async Task<ApiResponse> UserInfo()
        => await _accountManager.UserInfo(User);

        // DELETE: api/Account/5
        [HttpPost("UpdateUser")]
        [Authorize]
        public async Task<ApiResponse> UpdateUser(UserInfoDto userInfo)
        => ModelState.IsValid ? await _accountManager.UpdateUser(userInfo) : _invalidUserModel;

        ///----------Admin User Management Interface Methods
        // POST: api/Account/Create
        [HttpPost("Create")]
        [Authorize(Permissions.User.Create)]
        public async Task<ApiResponse> Create(RegisterDto parameters)
        => ModelState.IsValid ? await _accountManager.Create(parameters) : _invalidUserModel;

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
        public async Task<ApiResponse> Update([FromBody] UserInfoDto userInfo)
        => ModelState.IsValid ? await _accountManager.Update(userInfo) : _invalidUserModel;

        [HttpPost("AdminUserPasswordReset/{id}")]
        [Authorize(Permissions.User.Update)]
        [ProducesResponseType(Status204NoContent)]
        public async Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, [FromBody] string newPassword)
        => ModelState.IsValid
                ? await _accountManager.AdminResetUserPasswordAsync(id, newPassword, User)
                : _invalidUserModel;
    }
}
