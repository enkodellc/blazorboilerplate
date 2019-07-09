using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared;

namespace BlazorBoilerplate.Server.Models
{
    [Route("api/authorize")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private static UserInfo LoggedOutUser = new UserInfo {IsAuthenticated = false};

        // Logger instance
        ILogger<AuthorizeController> _logger;

        private readonly UserManager<ApplicationUser>    _userManager;
        private readonly SignInManager<ApplicationUser>  _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailService _emailService;

        public AuthorizeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ILogger<AuthorizeController> logger,
            RoleManager<IdentityRole<Guid>> roleManager, IEmailService emailService)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _logger        = logger;
            _roleManager   = roleManager;
            _emailService = emailService;
        }

        [HttpGet("")]
        public UserInfo GetUser()
        {
            return User.Identity.IsAuthenticated
                ? new UserInfo {Username = User.Identity.Name, IsAuthenticated = true}
                : LoggedOutUser;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login(LoginParameters parameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(state => state.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault());

            var user = await _userManager.FindByEmailAsync(parameters.UserName)
                       ?? await _userManager.FindByNameAsync(parameters.UserName);

            if (user == null)
            {
                _logger.LogInformation("User does not exist: {0}", parameters.UserName);
                return BadRequest("User does not exist");
            }

            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);

            if (!singInResult.Succeeded)
            {
                _logger.LogInformation("Invalid password: {0}, {1}", parameters.UserName, parameters.Password);
                return BadRequest("Invalid password");
            }

            _logger.LogInformation("Logged In: {0}, {1}", parameters.UserName, parameters.Password);

            // add custom claims here, before signin if needed
            var claims = await _userManager.GetClaimsAsync(user);
            //await _userManager.RemoveClaimsAsync(user, claims);

            await _signInManager.SignInAsync(user, parameters.RememberMe);
            return Ok(new { success = "true" });
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterParameters parameters)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState.Values.SelectMany(state => state.Errors)
                        .Select(error => error.ErrorMessage)
                        .FirstOrDefault());

                // https://gooroo.io/GoorooTHINK/Article/17333/Custom-user-roles-and-rolebased-authorization-in-ASPNET-core/32835
                string[] roleNames = { "SuperAdmin","Admin","User" };
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
                if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);

                //Role - Here we tie the new user to the "Admin" role
                await _userManager.AddToRoleAsync(user, "Admin");

                _logger.LogInformation("New user registered: {0}", user);

                #region Email New User
                try
                {
                  var email = new EmailMessage();
                  email.ToAddresses.Add(new EmailAddress(user.Email, user.Email));
                  email.FromAddresses.Add(new EmailAddress("support@blazorboilerplate.com", "support@blazorboilerplate.com"));
                  email = EmailTemplates.BuildNewUserEmail(email, user.UserName, user.UserName, user.Email); //Replace First UserName with Name if you want to add name to Registration Form
                  await _emailService.SendEmailAsync(email);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("New user email failed: {0}", ex.Message);
                }
                #endregion

                return await Login(new LoginParameters
                {
                    UserName = parameters.UserName,
                    Password = parameters.Password
                });
            }
            catch (Exception  ex)
            {
                _logger.LogError("Register User Failed: {0}", ex.Message);
                return BadRequest(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost("SendPasswordResetEmail")]
        public async Task<IActionResult> SendPasswordResetEmail(string emailAddress)
        {
            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                return Ok();
            }

            return Ok();

            // Todo Complete Email Service / Email Templates / Password reset

            //user.SetNewPasswordResetCode();
            //var passwordResetCode = user.PasswordResetCode;

            //#region Password Reset Email
            //try
            //{
            //    var email = new EmailMessage();
            //    email.ToAddresses.Add(new EmailAddress(user.Email, user.Email));
            //    email.FromAddresses.Add(new EmailAddress("support@blazorboilerplate.com", "support@blazorboilerplate.com"));
            //    email = EmailTemplates.BuildPasswordResetEmail(email, user.UserName, user.UserName, user.Email); //Replace First UserName with Name if you want to add name to Registration Form
            //    await _emailService.SendEmailAsync(email);
            //}
            //catch (Exception ex)
            //{

            //}
            //#endregion
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User Logged out");
            await _signInManager.SignOutAsync();
            return Ok();
        }
          
        //[Authorize]
        [HttpGet("userinfo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public UserInfo UserInfo()
        {
            return BuildUserInfo();
        }

        private UserInfo BuildUserInfo()
        {            
            return new UserInfo
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Username = User.Identity.Name,
                ExposedClaims = User.Claims
                        //Optionally: filter the claims you want to expose to the client
                        //.Where(c => c.Type == "test-claim")
                        .ToDictionary(c => c.Type, c => c.Value)
            };
        }
    }
}
