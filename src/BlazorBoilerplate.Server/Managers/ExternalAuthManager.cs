using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Controllers;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.ExternalAuth;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplate.Server.Managers
{
    // TODO: The methods are very tightly coupled with the controllers. Will re-visit. 
    public class ExternalAuthManager : IExternalAuthManager
    {
        private readonly IAccountManager _accountManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ExternalAuthManager> _logger;
        private readonly IConfiguration _configuration;

        public ExternalAuthManager(IAccountManager accountManager, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ILogger<ExternalAuthManager> logger,
            IConfiguration configuration)
        {
            _accountManager = accountManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
        }
        
        public async Task<(AuthenticationProperties authProps, string schemaName)> Challenge(string uri, string provider)
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var schema = schemes.SingleOrDefault(s => string.Compare(s.Name, provider, StringComparison.OrdinalIgnoreCase) == 0);

            if (schema == null)
            
                throw new ArgumentNullException($"~/externalauth/error/{ErrorEnum.ProviderNotFound}");

            var props = new AuthenticationProperties
            {
                RedirectUri = uri,
                Items =
                {
                    { "scheme", schema.Name },
                    { "returnUrl", uri }
                }
            };

            return (props, schema.Name); ;
        }

        public async Task<string> ExternalSignIn(HttpContext httpContext)
        {
            try
            {
                // read external identity from the temporary cookie
                var result = await httpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
                if (result?.Succeeded != true)
                {
                    return $"~/externalauth/error/{ErrorEnum.ExternalAuthError}";
                }

                // retrieve claims of the external user
                var externalUser = result.Principal;
                if (externalUser == null)
                {
                    return $"~/externalauth/error/{ErrorEnum.ExternalAuthError}";
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
                    return $"~/externalauth/error/{ErrorEnum.ExternalUnknownUserId}";
                }

                var externalUserId = userIdClaim.Value;
                var externalProvider = userIdClaim.Issuer;

                //Quick check to sign in
                var externalSignInResult = await _signInManager.ExternalLoginSignInAsync(externalProvider, externalUserId, true);
                if (externalSignInResult.Succeeded)
                {
                    //// delete temporary cookie used during external authentication
                    await httpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
                    return "~/externalauth/success";
                }

                //If external login/signin failed
                var userNameClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
                var userEmailClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                //get the user by Email (we are forcing it to be unique)
                var user = await _userManager.FindByEmailAsync(userEmailClaim.Value);
                if (user != null)
                {
                    //check if the login for this provider exists
                    var userLogins = await _userManager.GetLoginsAsync(user);
                    if (userLogins.Any(ul => ul.LoginProvider == externalProvider && ul.ProviderKey == externalUserId))
                    {
                        //something went wrong, it should get logged in

                        // If lock out activated and the max. amounts of attempts is reached.
                        if (externalSignInResult.IsLockedOut)
                        {
                            _logger.LogInformation("User Locked out: {0}", user.UserName);
                            return $"~/externalauth/error/{ErrorEnum.UserLockedOut}";
                        }

                        // If your email is not confirmed but you require it in the settings for login.
                        if (externalSignInResult.IsNotAllowed)
                        {
                            _logger.LogInformation("User not allowed to log in: {0}", user.UserName);
                            return $"~/externalauth/error/{ErrorEnum.UserIsNotAllowed}";
                        }

                        return $"~/externalauth/error/{ErrorEnum.Unknown}";
                    }

                }
                else // create new user first
                {
                    var requireConfirmEmail = Convert.ToBoolean(_configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false");
                    try
                    {
                        user = await _accountManager.RegisterNewUserAsync(userNameClaim.Value, userEmailClaim.Value, null, requireConfirmEmail);
                    }
                    catch (DomainException ex)
                    {
                        _logger.LogError("External login Failed: {0}, {1}", ex.Description, ex.Message);
                        return $"~/externalauth/error/{ErrorEnum.UserCreationFailed}/{ex.Description}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("External login Failed: " + ex.Message);
                        return $"~/externalauth/error/{ErrorEnum.UserCreationFailed}";
                    }

                    if (requireConfirmEmail)
                    {
                        //ToDo: suggestion - after confirm, the sign in might fail, user needs to relogin using the external provider
                        //Consider removing this constrain and auto-confirm the email - it has been confirmed by the 3rd party auth provider already
                        return $"~/externalauth/confirm";
                    }
                }

                
                //All if fine, this user (email) did not try to log in before using this external provider
                //Add external login info
                var addExternalLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(externalProvider, externalUserId, userNameClaim.Value));
                if (addExternalLoginResult.Succeeded == false)
                {
                    return $"~/externalauth/error/{ErrorEnum.CannotAddExternalLogin}";
                }

                //Try to sign in again
                externalSignInResult = await _signInManager.ExternalLoginSignInAsync(externalProvider, externalUserId, true);

                if (externalSignInResult.Succeeded)
                {
                    //// delete temporary cookie used during external authentication
                    await httpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
                    return "~/externalauth/success";
                }
                else
                {
                    //Something went terrible wrong, user exists, external login info added
                    // If lock out activated and the max. amounts of attempts is reached.
                    if (externalSignInResult.IsLockedOut)
                    {
                        _logger.LogInformation("User Locked out: {0}", user.UserName);
                        return $"~/externalauth/error/{ErrorEnum.UserLockedOut}";
                    }

                    // If your email is not confirmed but you require it in the settings for login.
                    if (externalSignInResult.IsNotAllowed)
                    {
                        _logger.LogInformation("User not allowed to log in: {0}", user.UserName);
                        return $"~/externalauth/error/{ErrorEnum.UserIsNotAllowed}";
                    }

                    return $"~/externalauth/error/{ErrorEnum.Unknown}";
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation("External login Failed: " + ex.Message);
                return $"~/externalauth/error/{ErrorEnum.Unknown}";
            }
        }
    }
}