using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.ExternalAuth;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
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
    public class ExternalAuthController : ControllerBase
    {
        private static readonly UserInfoDto LoggedOutUser = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };

        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        public ExternalAuthController(UserManager<ApplicationUser> userManager, ApplicationDbContext db,
            SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger,
            RoleManager<IdentityRole<Guid>> roleManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailService = emailService;
            _configuration = configuration;
            _db = db;
        }
        
        [HttpGet("challenge/{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> Challenge(string provider)
        {
            var callbackUrl = Url.RouteUrl("ExternalSignIn");
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var schema = schemes.SingleOrDefault(s => string.Compare(s.Name, provider, ignoreCase: true) == 0);

            if (schema == null)
            {
                return Redirect($"~/externalauth/error/{ErrorEnum.ProviderNotFound}");
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = callbackUrl,
                Items =
                {
                    { "scheme", schema.Name },
                    { "returnUrl", callbackUrl }
                }
            };

            return Challenge(props, schema.Name);
        }


        [HttpGet("SignIn", Name = "ExternalSignIn")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn()
        {
            try
            {
                // read external identity from the temporary cookie
                var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
                if (result?.Succeeded != true)
                {
                    return Redirect($"~/externalauth/error/{ErrorEnum.ExternalAuthError}");
                }

                // retrieve claims of the external user
                var externalUser = result.Principal;
                if (externalUser == null)
                {
                    return Redirect($"~/externalauth/error/{ErrorEnum.ExternalAuthError}");
                }

                // retrieve claims of the external user
                var claims = externalUser.Claims.ToList();

                // try to determine the unique id of the external user - the most common claim type for that are the sub claim and the NameIdentifier
                // depending on the external provider, some other claim type might be used
                var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
                if (userIdClaim == null)
                {
                    userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                }
                if (userIdClaim == null)
                {
                    return Redirect($"~/externalauth/error/{ErrorEnum.ExternalUnknownUserId}");
                }

                var externalUserId = userIdClaim.Value;
                var externalProvider = userIdClaim.Issuer;

                //Quick check to sign in
                var externalSignInResult = await _signInManager.ExternalLoginSignInAsync(externalProvider, externalUserId, true);
                if (externalSignInResult.Succeeded)
                {
                    //// delete temporary cookie used during external authentication
                    await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
                    return Redirect("~/externalauth/success");
                }

                //If external login signin failed, start the fun part
                var userNameClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
                var userEmailClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                //get the user by Email (we are forcing it to be unique)
                var user = await _userManager.FindByEmailAsync(userEmailClaim.Value);

                // ToDO, duplicate code from Account/Register
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = userNameClaim.Value,
                        Email = userEmailClaim.Value
                    };

                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        return Redirect($"~/externalauth/error/{ErrorEnum.UserCreationFailed}");
                    }

                    var claimsResult = await _userManager.AddClaimsAsync(user, new Claim[]{
                        new Claim(Policies.IsUser,""),
                        new Claim(JwtClaimTypes.Name, user.UserName),
                        new Claim(JwtClaimTypes.Email, user.Email),
                        new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                    });

                    //Role - Here we tie the new user to the "User" role
                    await _userManager.AddToRoleAsync(user, "User");

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
                    }

                    #region New  User Email
                    try
                    {
                        var email = new EmailMessageDto();
                        email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                        email = EmailTemplates.BuildNewUserEmail(email, user.FullName, user.UserName, user.Email, "");

                        _logger.LogInformation("New user registered: {0}", user);
                        await _emailService.SendEmailAsync(email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("New user email failed: {0}", ex.Message);
                    }
                    #endregion
                }

                //check if the login for this provider exists
                var userLogins = await _userManager.GetLoginsAsync(user);
                if (userLogins.Any(ul => ul.LoginProvider == externalProvider && ul.ProviderKey == externalUserId))
                {
                    //something went wrong, it should get logged in

                    // If lock out activated and the max. amounts of attempts is reached.
                    if (externalSignInResult.IsLockedOut)
                    {
                        _logger.LogInformation("User Locked out: {0}", user.UserName);
                        return Redirect($"~/externalauth/error/{ErrorEnum.UserLockedOut}");
                    }

                    // If your email is not confirmed but you require it in the settings for login.
                    if (externalSignInResult.IsNotAllowed)
                    {
                        _logger.LogInformation("User not allowed to log in: {0}", user.UserName);
                        return Redirect($"~/externalauth/error/{ErrorEnum.UserIsNotAllowed}");
                    }

                    return Redirect($"~/externalauth/error/{ErrorEnum.Unknown}");
                }

                //All if fine, this user (email) did not try to log in before using this external provider
                //Add external login info
                var addExternalLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(externalProvider, externalUserId, userNameClaim.Value));
                if (addExternalLoginResult.Succeeded == false)
                {
                    return Redirect($"~/externalauth/error/{ErrorEnum.CannotAddExternalLogin}");
                }

                //Try to sign in again
                externalSignInResult = await _signInManager.ExternalLoginSignInAsync(externalProvider, externalUserId, true);

                if (externalSignInResult.Succeeded == false)
                {
                    //Something went terrible wrong, user exists, external login info added
                    // If lock out activated and the max. amounts of attempts is reached.
                    if (externalSignInResult.IsLockedOut)
                    {
                        _logger.LogInformation("User Locked out: {0}", user.UserName);
                        return Redirect($"~/externalauth/error/{ErrorEnum.UserLockedOut}");
                    }

                    // If your email is not confirmed but you require it in the settings for login.
                    if (externalSignInResult.IsNotAllowed)
                    {
                        _logger.LogInformation("User not allowed to log in: {0}", user.UserName);
                        return Redirect($"~/externalauth/error/{ErrorEnum.UserIsNotAllowed}");
                    }

                    return Redirect($"~/externalauth/error/{ErrorEnum.Unknown}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("External login Failed: " + ex.Message);
            }

            return Redirect($"~/externalauth/error/{ErrorEnum.Unknown}");
        }

    }
}
