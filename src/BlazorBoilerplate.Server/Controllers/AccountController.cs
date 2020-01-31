using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager _accountManager;

        private readonly ApiResponse _invalidUserModel;

        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager;
            _invalidUserModel = new ApiResponse(400, "User Model is Invalid"); // Could we inject this? As some form of 'Errors which has constant values'?
        }

        // POST: api/Account/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ApiResponse> Login(LoginDto parameters)
            => ModelState.IsValid ? await _accountManager.Login(parameters) : _invalidUserModel;

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
        =>  ModelState.IsValid ? await _accountManager.ResetPassword(parameters) : _invalidUserModel;

        // POST: api/Account/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<ApiResponse> Logout()
            => await _accountManager.Logout();

        [HttpGet("UserInfo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ApiResponse> UserInfo()
        =>  await _accountManager.UserInfo(User);

        // DELETE: api/Account/5
        [HttpPost("UpdateUser")]
        [Authorize]
        public async Task<ApiResponse> UpdateUser(UserInfoDto userInfo)
        => ModelState.IsValid ? await _accountManager.UpdateUser(userInfo) : _invalidUserModel;
        
        ///----------Admin User Management Interface Methods
        // POST: api/Account/Create
        [HttpPost("Create")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> Create(RegisterDto parameters)
        =>  ModelState.IsValid ? await _accountManager.Create(parameters) : _invalidUserModel;

        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> Delete(string id)
        => await _accountManager.Delete(id);

        [HttpGet("GetUser")]
        public ApiResponse GetUser()
        => _accountManager.GetUser(User);

        [HttpGet("ListRoles")]
        [Authorize]
        public async Task<ApiResponse> ListRoles()
        =>  await _accountManager.ListRoles();

        [HttpPut]
        [Authorize(Policy = Policies.IsAdmin)]
        // PUT: api/Account/5
        public async Task<ApiResponse> Update([FromBody] UserInfoDto userInfo)
        =>  ModelState.IsValid ? await _accountManager.Update(userInfo) : _invalidUserModel;

        [HttpPost("AdminUserPasswordReset/{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        [ProducesResponseType(204)]
        public async Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, [FromBody] string newPassword)
        => ModelState.IsValid
                ? await _accountManager.AdminResetUserPasswordAsync(id, newPassword, User)
                : _invalidUserModel;
    }
}