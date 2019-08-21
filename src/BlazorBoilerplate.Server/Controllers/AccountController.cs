using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private static readonly UserInfoDto LoggedOutUser = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };

        // Logger instance
        private readonly ILogger<AccountController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger,
            RoleManager<IdentityRole<Guid>> roleManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet("GetUser")]
        [Authorize]
        public ApiResponse GetUser()
        {
            UserInfoDto userInfo = User != null && User.Identity.IsAuthenticated
                ? new UserInfoDto { UserName = User.Identity.Name, IsAuthenticated = true }
                : LoggedOutUser;
            return new ApiResponse(200, "Get User Successful", userInfo);
        }

        [Authorize]
        // GET: api/Account/User
        [HttpGet]
        public async Task<ApiResponse> Get()
        {
            var users = _userManager.Users;
            return new ApiResponse(200, "Get User Successful", users);
        }

        // POST: api/Account/Login
        [HttpPost("Login")]

        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<ApiResponse> Login(LoginDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByEmailAsync(parameters.UserName)
                       ?? await _userManager.FindByNameAsync(parameters.UserName);

            if (!user.EmailConfirmed && Convert.ToBoolean(_configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false"))
            {
                return new ApiResponse(401, "User has not confirmed their email.");
            }

            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserName);
                return new ApiResponse(404, "User does not exist");
            }

            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);

            if (!singInResult.Succeeded)
            {
                _logger.LogInformation("Invalid password: {0}, {1}", parameters.UserName, parameters.Password);
                return new ApiResponse(400, "Invalid password");
            }

            _logger.LogInformation("Logged In: {0}, {1}", parameters.UserName, parameters.Password);

            // add custom claims here, before signin if needed
            //var claims = await _userManager.GetClaimsAsync(user);
            //await _userManager.RemoveClaimsAsync(user, claims);
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _signInManager.SignInAsync(user, parameters.RememberMe);
            return new ApiResponse(200, "Login Successful");
        }

        [AllowAnonymous]
        // POST: api/Account/Register
        [HttpPost("Register")]
        public async Task<ApiResponse> Register(RegisterDto parameters)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ApiResponse(400, "User Model is Invalid");
                }

                // https://gooroo.io/GoorooTHINK/Article/17333/Custom-user-roles-and-rolebased-authorization-in-ASPNET-core/32835
                string[] roleNames = { "SuperAdmin", "Admin", "User" };
                IdentityResult roleResult;

                foreach (var roleName in roleNames)
                {
                    //creating the roles and seeding them to the database
                    var roleExist = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = parameters.UserName,
                    Email = parameters.Email
                };

                user.UserName = parameters.UserName;
                var result = await _userManager.CreateAsync(user, parameters.Password);
                if (!result.Succeeded)
                {
                    return new ApiResponse(400, "Register User Failed: " +  result.Errors.FirstOrDefault()?.Description);
                }


                //Role - Here we tie the new user to the "Admin" role
                await _userManager.AddToRoleAsync(user, "Admin");

                if (Convert.ToBoolean(_configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false"))
                {
                    #region New  User Confirmation Email
                    try
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        string callbackUrl = string.Format("{0}/Account/ConfirmEmail/{1}?token={2}", _configuration["BlazorBoilerplate:ApplicationUrl"], user.Id, token);

                        var email = new EmailMessageDto();
                        email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                        email = EmailTemplates.BuildNewUserConfirmationEmail(email, user.UserName, user.Email, callbackUrl, user.Id.ToString(), token); //Replace First UserName with Name if you want to add name to Registration Form

                        _logger.LogInformation("New user registered: {0}", user);
                        await _emailService.SendEmailAsync(email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("New user email failed: {0}", ex.Message);
                    }
                    #endregion
                    return new ApiResponse(200, "Register User Success");
                }

                #region New  User Email
                try
                {
                    var email = new EmailMessageDto();
                    email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                    email = EmailTemplates.BuildNewUserEmail(email, user.UserName, user.Email, parameters.Password); //Replace First UserName with Name if you want to add name to Registration Form

                    _logger.LogInformation("New user registered: {0}", user);
                    await _emailService.SendEmailAsync(email);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("New user email failed: {0}", ex.Message);
                }
                #endregion

                return await Login(new LoginDto
                {
                    UserName = parameters.UserName,
                    Password = parameters.Password
                });

            }
            catch (Exception ex)
            {
                _logger.LogError("Register User Failed: {0}", ex.Message);
                return new ApiResponse(400, "Register User Failed");
            }
        }

        [AllowAnonymous]
        // POST: api/Account/ConfirmEmail
        [HttpPost("ConfirmEmail")]
        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "User Model is Invalid");
            }

            if (parameters.UserId == null || parameters.Token == null)
            {
                return new ApiResponse(404, "User does not exist");
            }

            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserId);
                return new ApiResponse(404, "User does not exist");
            }

            string token = parameters.Token;
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                _logger.LogInformation("User Email Confirmation Failed: {0}", result.Errors.FirstOrDefault()?.Description);
                return new ApiResponse(400, "User Email Confirmation Failed");
            }

            await _signInManager.SignInAsync(user, true);

            return new ApiResponse(200, "Success");
        }

        [AllowAnonymous]
        // POST: api/Account/ForgotPassword
        [HttpPost("ForgotPassword")]
        public async Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByEmailAsync(parameters.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                _logger.LogInformation("Forgot Password with non-existent email / user: {0}", parameters.Email);
                // Don't reveal that the user does not exist or is not confirmed
                return new ApiResponse(200, "Success");
            }

            #region Forgot Password Email
            try
            {
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = string.Format("{0}/Account/ResetPassword/{1}?token={2}", _configuration["BlazorBoilerplate:ApplicationUrl"], user.Id, token); //token must be a query string parameter as it is very long

                var email = new EmailMessageDto();
                email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                email = EmailTemplates.BuildForgotPasswordEmail(email, user.UserName, callbackUrl, token); //Replace First UserName with Name if you want to add name to Registration Form

                _logger.LogInformation("Forgot Password Email Sent: {0}", user.Email);
                await _emailService.SendEmailAsync(email);
                return new ApiResponse(200, "Forgot Password Email Sent");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Forgot Password email failed: {0}", ex.Message);
            }
            #endregion
            return new ApiResponse(200, "Success");
        }

        [AllowAnonymous]
        // PUT: api/Account/ResetPassword
        [HttpPost("ResetPassword")]
        public async Task<ApiResponse> ResetPassword(ResetPasswordDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserId);
                return new ApiResponse(404, "User does not exist");
            }

            #region Reset Password Successful Email
            try
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(user, parameters.Token, parameters.Password);
                if (result.Succeeded)
                {
                    #region Email Successful Password change
                    var email = new EmailMessageDto();
                    email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                    email = EmailTemplates.BuildPasswordResetEmail(email, user.UserName); //Replace First UserName with Name if you want to add name to Registration Form

                    _logger.LogInformation("Reset Password Successful Email Sent: {0}", user.Email);
                    await _emailService.SendEmailAsync(email);
                    #endregion

                    return new ApiResponse(200, String.Format("Reset Password Successful Email Sent: {0}", user.Email));
                }
                else
                {
                    _logger.LogInformation("Error while resetting the password!: {0}", user.UserName);
                    return new ApiResponse(400, string.Format("Error while resetting the password!: {0}", user.UserName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Reset Password failed: {0}", ex.Message);
                return new ApiResponse(400, string.Format("Error while resetting the password!: {0}", ex.Message));

            }
            #endregion
        }

        [Authorize]
        // POST: api/Account/Logout
        [HttpPost("Logout")]
        public async Task<ApiResponse> Logout()
        {
            await _signInManager.SignOutAsync();
            return new ApiResponse(200, "Logout Successful");
        }

        //[Authorize]
        [HttpGet("UserInfo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ApiResponse> UserInfo()
        {
            UserInfoDto userInfo = await BuildUserInfo();
            return new ApiResponse(200, "Retrieved UserInfo", userInfo); ;
        }

        private async Task<UserInfoDto> BuildUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);

            return new UserInfoDto
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExposedClaims = User.Claims
                        //Optionally: filter the claims you want to expose to the client
                        //.Where(c => c.Type == "test-claim")
                        .ToDictionary(c => c.Type, c => c.Value),
                Roles = ((ClaimsIdentity)User.Identity).Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value).ToList()
            };
        }

        [AllowAnonymous]
        // DELETE: api/Account/5
        [HttpPost("UpdateUser")]
        public async Task<ApiResponse> UpdateUser(UserInfoDto userInfo)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", userInfo.Email);
                return new ApiResponse(404, "User does not exist");
            }

            user.FirstName = userInfo.FirstName;
            user.LastName = userInfo.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogInformation("User Update Failed: {0}", result.Errors.FirstOrDefault()?.Description);
                return new ApiResponse(400, "User Update Failed");
            }

            return new ApiResponse(200, "User Updated Successfully");
        }


        [Authorize]
        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        public async Task<ApiResponse> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new ApiResponse(404, "User does not exist");
            }

            await _userManager.DeleteAsync(user);

            return new ApiResponse(200, "User Deletion Successful");
        }

    }
}
