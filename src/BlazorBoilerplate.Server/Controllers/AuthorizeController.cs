using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Middleware.Wrappers;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private static readonly UserInfoDto LoggedOutUser = new UserInfoDto { IsAuthenticated = false, Roles = new String[] { } };

        // Logger instance
        private readonly ILogger<AuthorizeController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthorizeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ILogger<AuthorizeController> logger,
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
        public APIResponse GetUser()
        {
            UserInfoDto userInfo = User != null && User.Identity.IsAuthenticated
                ? new UserInfoDto { UserName = User.Identity.Name, IsAuthenticated = true }
                : LoggedOutUser;
            return new APIResponse(200, "Get User Successful", userInfo);
        }

        //[HttpGet("")]
        //[Authorize]
        //public List<UserInfo> GetUsers()
        //{

        //}

        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<APIResponse> Login(LoginDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByEmailAsync(parameters.UserName)
                       ?? await _userManager.FindByNameAsync(parameters.UserName);

            if (!user.EmailConfirmed && Convert.ToBoolean(_configuration["BlazorBoilerplate.RequireConfirmedEmail"]))
            {
                return new APIResponse(401, "User has not confirmed their email.");
            }

            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserName);
                return new APIResponse(404, "User does not exist");
            }

            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);

            if (!singInResult.Succeeded)
            {
                _logger.LogInformation("Invalid password: {0}, {1}", parameters.UserName, parameters.Password);
                return new APIResponse(400, "Invalid password");
            }

            _logger.LogInformation("Logged In: {0}, {1}", parameters.UserName, parameters.Password);

            // add custom claims here, before signin if needed
            var claims = await _userManager.GetClaimsAsync(user);
            //await _userManager.RemoveClaimsAsync(user, claims);

            await _signInManager.SignInAsync(user, parameters.RememberMe);
            return new APIResponse(200, "Login Successful");
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<APIResponse> Register(RegisterDto parameters)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse(400, "User Model is Invalid");
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
                    return new APIResponse(400, "Register User Failed: " +  result.Errors.FirstOrDefault()?.Description);
                }


                //Role - Here we tie the new user to the "Admin" role
                await _userManager.AddToRoleAsync(user, "Admin");

                if (Convert.ToBoolean(_configuration["BlazorBoilerplate.RequireConfirmedEmail"]))
                {
                    #region New  User Confirmation Email
                    try
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        string callbackUrl = string.Format("{0}/Account/ConfirmEmail/{1}?token={2}", _configuration["BlazorBoilerplate.ApplicationUrl"], user.Id, token);

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
                    return new APIResponse(200, "Register User Success");
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
                return new APIResponse(400, "Register User Failed");
            }
        }

        [AllowAnonymous]
        [HttpPost("ConfirmEmail")]
        public async Task<APIResponse> ConfirmEmail(ConfirmEmailDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "User Model is Invalid");
            }

            if (parameters.UserId == null || parameters.Token == null)
            {
                return new APIResponse(404, "User does not exist");
            }

            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserId);
                return new APIResponse(404, "User does not exist");
            }

            string token = parameters.Token;
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                _logger.LogInformation("User Email Confirmation Failed: {0}", result.Errors.FirstOrDefault()?.Description);
                return new APIResponse(400, "User Email Confirmation Failed");
            }

            await _signInManager.SignInAsync(user, true);

            return new APIResponse(200, "Success");
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<APIResponse> ForgotPassword(ForgotPasswordDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByEmailAsync(parameters.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                _logger.LogInformation("Forgot Password with non-existent email / user: {0}", parameters.Email);
                // Don't reveal that the user does not exist or is not confirmed
                return new APIResponse(200, "Success");
            }

            #region Forgot Password Email
            try
            {
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = string.Format("{0}/Account/ResetPassword/{1}?token={2}", _configuration["BlazorBoilerplate.ApplicationUrl"], user.Id, token); //token must be a query string parameter as it is very long

                var email = new EmailMessageDto();
                email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                email = EmailTemplates.BuildForgotPasswordEmail(email, user.UserName, callbackUrl, token); //Replace First UserName with Name if you want to add name to Registration Form

                _logger.LogInformation("Forgot Password Email Sent: {0}", user.Email);
                await _emailService.SendEmailAsync(email);
                return new APIResponse(200, "Forgot Password Email Sent");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Forgot Password email failed: {0}", ex.Message);
            }
            #endregion
            return new APIResponse(200, "Success");
        }

        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [HttpPost("ResetPassword")]
        public async Task<APIResponse> ResetPassword(ResetPasswordDto parameters)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserId);
                return new APIResponse(404, "User does not exist");
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

                    return new APIResponse(200, String.Format("Reset Password Successful Email Sent: {0}", user.Email));
                }
                else
                {
                    _logger.LogInformation("Error while resetting the password!: {0}", user.UserName);
                    return new APIResponse(400, string.Format("Error while resetting the password!: {0}", user.UserName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Reset Password failed: {0}", ex.Message);
                return new APIResponse(400, string.Format("Error while resetting the password!: {0}", ex.Message));

            }
            #endregion
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<APIResponse> Logout()
        {
            _logger.LogInformation("User Logged out");
            await _signInManager.SignOutAsync();
            return new APIResponse(200, "Logout Successful");
        }

        //[Authorize]
        [HttpGet("UserInfo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<APIResponse> UserInfo()
        {
            UserInfoDto userInfo = await BuildUserInfo();
            return new APIResponse(200, "Retrieved UserInfo", userInfo); ;
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
                        .Select(c => c.Value).ToArray()
            };
        }

        [AllowAnonymous]
        [HttpPost("UpdateUser")]
        public async Task<APIResponse> UpdateUser(UserInfoDto userInfo)
        {
            if (!ModelState.IsValid)
            {
                return new APIResponse(400, "User Model is Invalid");
            }

            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", userInfo.Email);
                return new APIResponse(404, "User does not exist");
            }

            user.FirstName = userInfo.FirstName;
            user.LastName = userInfo.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogInformation("User Update Failed: {0}", result.Errors.FirstOrDefault()?.Description);
                return new APIResponse(400, "User Update Failed");
            }

            return new APIResponse(200, "User Updated Successfully");
        }
    }
}
