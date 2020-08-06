using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Shared.Providers;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Text;
using System.Text.Encodings.Web;

namespace BlazorBoilerplate.Server.Managers
{
    [ApiResponseException]
    public class AccountManager : IAccountManager
    {
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountManager> _logger;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailManager _emailManager;
        private readonly IClientStore _clientStore;
        private readonly IConfiguration _configuration;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly UrlEncoder _urlEncoder;
        private readonly IEventService _events;
        private readonly IStringLocalizer<Strings> L;

        private static readonly UserInfo LoggedOutUser = new UserInfo { IsAuthenticated = false, Roles = new List<string>() };

        public AccountManager(IDatabaseInitializer databaseInitializer,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountManager> logger,
            RoleManager<IdentityRole<Guid>> roleManager,
            IEmailManager emailManager,
            IClientStore clientStore,
            IConfiguration configuration,
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            UrlEncoder urlEncoder,
            IEventService events,
            IStringLocalizer<Strings> l)
        {
            _databaseInitializer = databaseInitializer;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailManager = emailManager;
            _clientStore = clientStore;
            _configuration = configuration;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _urlEncoder = urlEncoder;
            _events = events;
            L = l;
        }

        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters)
        {
            if (parameters.UserId == null || parameters.Token == null)
            {
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", parameters.UserId]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var token = parameters.Token;
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var msg = string.Join(",", result.Errors.Select(i => i.Description));
                _logger.LogWarning("User Email Confirmation Failed: {0}", msg);
                return new ApiResponse(Status400BadRequest, msg);
            }

            await _signInManager.SignInAsync(user, true);

            return new ApiResponse(Status200OK, L["EmailVerificationSuccessful"]);
        }

        public async Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters)
        {
            var user = await _userManager.FindByEmailAsync(parameters.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogInformation("Forgot Password with non-existent email / user: {0}", parameters.Email);
                // Don't reveal that the user does not exist or is not confirmed
                return new ApiResponse(Status200OK, L["Operation Successful"]);
            }

            // TODO: Break out the email sending here, to a separate class/service etc..
            #region Forgot Password Email

            try
            {
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = string.Format("{0}/Account/ResetPassword/{1}?token={2}", _configuration["BlazorBoilerplate:ApplicationUrl"], user.Id, token); //token must be a query string parameter as it is very long

                var email = new EmailMessageDto();
                email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                email.BuildForgotPasswordEmail(user.UserName, callbackUrl, token); //Replace First UserName with Name if you want to add name to Registration Form

                _logger.LogInformation("Forgot Password Email Sent: {0}", user.Email);
                await _emailManager.SendEmailAsync(email);
                return new ApiResponse(Status200OK, "Forgot Password Email Sent");
            }
            catch (Exception ex)
            {
                _logger.LogError("Forgot Password email failed: {0}", ex.GetBaseException().Message);
            }

            #endregion Forgot Password Email

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        public async Task<ApiResponse> BuildLoginViewModel(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    UserName = context?.LoginHint
                };

                if (!local)
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };

                return new ApiResponse(Status200OK, L["Operation Successful"], vm);
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase)
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new ApiResponse(Status200OK, L["Operation Successful"], new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                UserName = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            });
        }

        public async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(ClaimsPrincipal user, HttpContext httpContext, string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (user?.Identity.IsAuthenticated == true)
            {
                var idp = user.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await httpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
        public async Task<ApiResponse> Login(LoginInputModel parameters)
        {
            try
            {
                await _databaseInitializer.EnsureAdminIdentitiesAsync();

                var context = await _interaction.GetAuthorizationContextAsync(parameters.ReturnUrl);

                var result = await _signInManager.PasswordSignInAsync(parameters.UserName, parameters.Password, parameters.RememberMe, true);

                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("Two factor authentication required for user {0}", parameters.UserName);

                    return new ApiResponse(Status200OK, "Two factor authentication required")
                    {
                        Result = new LoginResponseModel()
                        {
                            RequiresTwoFactor = true
                        }
                    };
                }

                // If lock out activated and the max. amounts of attempts is reached.
                if (result.IsLockedOut)
                {
                    _logger.LogInformation("User Locked out: {0}", parameters.UserName);
                    return new ApiResponse(Status401Unauthorized, L["LockedUser"]);
                }

                // If your email is not confirmed but you require it in the settings for login.
                if (result.IsNotAllowed)
                {
                    _logger.LogInformation("User {0} not allowed to log in, because email is not confirmed", parameters.UserName);
                    return new ApiResponse(Status401Unauthorized, L["EmailNotConfirmed"]);
                }

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(parameters.UserName);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client?.ClientId));
                    _logger.LogInformation("Logged In user {0}", parameters.UserName);

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            //return this.LoadingPage("Redirect", model.ReturnUrl);
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        //return Redirect(model.ReturnUrl);
                    }

                    //TODO parameters.IsValidReturnUrl is set true above 
                    //if (!parameters.IsValidReturnUrl)
                    //    // user might have clicked on a malicious link - should be logged
                    //    throw new Exception("invalid return URL");

                    return new ApiResponse(Status200OK);
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(parameters.UserName, "Invalid Password for user {0}", clientId: context?.Client.ClientId));
                _logger.LogInformation("Invalid Password for user {0}", parameters.UserName);
                return new ApiResponse(Status401Unauthorized, L["LoginFailed"]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login Failed: {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, L["LoginFailed"]);
            }
        }
        public async Task<ApiResponse> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            try
            {
                // Ensure the user has gone through the username & password screen first
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

                if (user == null)
                {
                    return new ApiResponse(Status404NotFound, "Unable to load two-factor authentication user.");
                }

                var authenticatorCode = parameters.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

                var context = await _interaction.GetAuthorizationContextAsync(parameters.ReturnUrl);

                var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, parameters.RememberMe, parameters.RememberMachine);

                // If lock out activated and the max. amounts of attempts is reached.
                if (result.IsLockedOut)
                {
                    _logger.LogInformation("User Locked out: {0}", user.UserName);
                    return new ApiResponse(Status401Unauthorized, L["LockedUser"]);
                }

                // If your email is not confirmed but you require it in the settings for login.
                if (result.IsNotAllowed)
                {
                    _logger.LogInformation("User {0} not allowed to log in, because email is not confirmed", user.UserName);
                    return new ApiResponse(Status401Unauthorized, L["EmailNotConfirmed"]);
                }

                if (result.Succeeded)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client?.ClientId));
                    _logger.LogInformation("User '{0}' logged in with a authenticator code", user.UserName);

                    return new ApiResponse(Status200OK);
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(user.UserName, "Invalid authenticator code for user {0}", clientId: context?.Client.ClientId));
                _logger.LogInformation("Invalid authenticator code for user {0}", user.UserName);
                return new ApiResponse(Status401Unauthorized, L["LoginFailed"]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login Failed: {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, L["LoginFailed"]);
            }
        }
        public async Task<ApiResponse> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            try
            {
                // Ensure the user has gone through the username & password screen first
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

                if (user == null)
                {
                    return new ApiResponse(Status404NotFound, "Unable to load two-factor authentication user.");
                }

                var recoveryCode = parameters.RecoveryCode.Replace(" ", string.Empty);

                var context = await _interaction.GetAuthorizationContextAsync(parameters.ReturnUrl);

                var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

                // If lock out activated and the max. amounts of attempts is reached.
                if (result.IsLockedOut)
                {
                    _logger.LogInformation("User Locked out: {0}", user.UserName);
                    return new ApiResponse(Status401Unauthorized, L["LockedUser"]);
                }

                // If your email is not confirmed but you require it in the settings for login.
                if (result.IsNotAllowed)
                {
                    _logger.LogInformation("User {0} not allowed to log in, because email is not confirmed", user.UserName);
                    return new ApiResponse(Status401Unauthorized, L["EmailNotConfirmed"]);
                }

                if (result.Succeeded)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client?.ClientId));
                    _logger.LogInformation("User '{0}' logged in with a recovery code", user.UserName);

                    return new ApiResponse(Status200OK);
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(user.UserName, "Invalid recovery code for user {0}", clientId: context?.Client.ClientId));
                _logger.LogInformation("Invalid recovery code for user {0}", user.UserName);
                return new ApiResponse(Status401Unauthorized, L["LoginFailed"]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login Failed: {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, L["LoginFailed"]);
            }
        }
        public async Task<ApiResponse> Logout(ClaimsPrincipal user)
        {
            if (user?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(user.GetSubjectId(), user.GetDisplayName()));
            }

            return new ApiResponse(Status200OK, "Logout Successful");
        }

        public async Task<ApiResponse> Register(RegisterDto parameters)
        {
            var requireConfirmEmail = Convert.ToBoolean(_configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false");

            await RegisterNewUserAsync(parameters.UserName, parameters.Email, parameters.Password, requireConfirmEmail);

            if (requireConfirmEmail)
            {
                return new ApiResponse(Status200OK, L["Operation Successful"]);
            }
            else
            {
                return await Login(new LoginInputModel
                {
                    UserName = parameters.UserName,
                    Password = parameters.Password
                });
            }
        }

        public async Task<ApiResponse> ResetPassword(ResetPasswordDto parameters)
        {
            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", parameters.UserId]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            // TODO: Break this out into it's own self-contained Email Helper service.

            var result = await _userManager.ResetPasswordAsync(user, parameters.Token, parameters.Password);
            if (result.Succeeded)
            {
                #region Email Successful Password change

                var email = new EmailMessageDto();
                email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                email.BuildPasswordResetEmail(user.UserName); //Replace First UserName with Name if you want to add name to Registration Form

                _logger.LogInformation($"Reset Password Successful Email Sent: {user.Email}");
                await _emailManager.SendEmailAsync(email);

                #endregion Email Successful Password change

                return new ApiResponse(Status200OK, $"Reset Password Successful Email Sent: {user.Email}");
            }
            else
            {
                var msg = string.Join(",", result.Errors.Select(i => i.Description));
                _logger.LogWarning("Error while resetting the password: {0}", msg);
                return new ApiResponse(Status400BadRequest, msg);
            }
        }

        public async Task<ApiResponse> UpdatePassword(ClaimsPrincipal userClaimsPrincipal, UpdatePasswordDto parameters)
        {
            var user = await _userManager.FindByIdAsync(userClaimsPrincipal.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userClaimsPrincipal.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.ChangePasswordAsync(user, parameters.CurrentPassword, parameters.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return new ApiResponse(Status200OK, L["Operation Successful"]);
            }
            else
            {
                var msg = string.Join(",", result.Errors.Select(i => i.Description));
                _logger.LogWarning($"Error while updating the password of {user.UserName}: {msg}");
                return new ApiResponse(Status400BadRequest, msg);
            }
        }

        public async Task<ApiResponse> EnableAuthenticator(ClaimsPrincipal userClaimsPrincipal, AuthenticatorVerificationCodeDto parameters)
        {
            var user = await _userManager.FindByIdAsync(userClaimsPrincipal.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userClaimsPrincipal.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var verificationCode = parameters.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2faTokenValid)
            {
                var result = await _userManager.SetTwoFactorEnabledAsync(user, true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User '{0}' has enabled 2FA with an authenticator app.", user.UserName);

                    var userInfo = await BuildUserInfo(userClaimsPrincipal);

                    if (await _userManager.CountRecoveryCodesAsync(user) == 0)
                    {
                        userInfo.RecoveryCodes = (await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).ToArray();
                    }

                    return new ApiResponse(Status200OK, L["Operation Successful"], userInfo);
                }
                else
                    return new ApiResponse(Status400BadRequest, "Error while enabling 2FA");
            }
            else
            {
                _logger.LogWarning($"Verification code of {user.UserName} is invalid.");
                return new ApiResponse(Status400BadRequest, L["VerificationCodeInvalid"]);
            }
        }
        public async Task<ApiResponse> DisableAuthenticator(ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.FindByIdAsync(userClaimsPrincipal.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userClaimsPrincipal.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

            if (result.Succeeded)
            {
                result = await _userManager.ResetAuthenticatorKeyAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

                    await _signInManager.RefreshSignInAsync(user);
                }
                else
                    return new ApiResponse(Status400BadRequest, "Error while disabling authenticator");
            }
            else
                return new ApiResponse(Status400BadRequest, "Error while disabling 2fa");

            return new ApiResponse(Status200OK, L["Operation Successful"], await BuildUserInfo(userClaimsPrincipal));
        }
        public async Task<ApiResponse> ForgetTwoFactorClient(ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.FindByIdAsync(userClaimsPrincipal.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userClaimsPrincipal.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            await _signInManager.ForgetTwoFactorClientAsync();

            return new ApiResponse(Status200OK, L["Operation Successful"], await BuildUserInfo(userClaimsPrincipal));
        }
        public async Task<ApiResponse> Enable2fa(ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.FindByIdAsync(userClaimsPrincipal.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userClaimsPrincipal.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);

            if (result.Succeeded)
            {
                return new ApiResponse(Status200OK, "Enabling 2fa Successful", await BuildUserInfo(userClaimsPrincipal));
            }
            else
                return new ApiResponse(Status400BadRequest, "Error while enabling 2fa");
        }
        public async Task<ApiResponse> Disable2fa(ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.FindByIdAsync(userClaimsPrincipal.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userClaimsPrincipal.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

            if (result.Succeeded)
            {
                return new ApiResponse(Status200OK, "Disabling 2fa Successful", await BuildUserInfo(userClaimsPrincipal));
            }
            else
                return new ApiResponse(Status400BadRequest, "Error while disabling 2fa");
        }

        public async Task<ApiResponse> UserInfo(ClaimsPrincipal user)
        {
            var userInfo = await BuildUserInfo(user);
            return new ApiResponse(Status200OK, L["Operation Successful"], userInfo);
        }
        public async Task<ApiResponse> UpdateUser(UserInfo userInfo)
        {
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userInfo.Email]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            user.FirstName = userInfo.FirstName;
            user.LastName = userInfo.LastName;
            user.Email = userInfo.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var msg = string.Join(",", result.Errors.Select(i => i.Description));
                _logger.LogWarning("User Update Failed: {0}", msg);
                return new ApiResponse(Status400BadRequest, msg);
            }

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        public async Task<ApiResponse> Create(RegisterDto parameters)
        {
            var user = new ApplicationUser
            {
                UserName = parameters.UserName,
                Email = parameters.Email
            };

            user.UserName = parameters.UserName;
            var result = await _userManager.CreateAsync(user, parameters.Password);
            if (!result.Succeeded)
            {
                var msg = string.Join(",", result.Errors.Select(i => i.Description));
                _logger.LogWarning($"Error while creating {user.UserName}: {msg}");
                return new ApiResponse(Status400BadRequest, msg);
            }
            else
            {
                var claimsResult = _userManager.AddClaimsAsync(user, new Claim[]{
                        new Claim(Policies.IsUser, string.Empty),
                        new Claim(JwtClaimTypes.Name, parameters.UserName),
                        new Claim(JwtClaimTypes.Email, parameters.Email),
                        new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                    }).Result;
            }

            var defaultRoleExists = await _roleManager.RoleExistsAsync(DefaultRoleNames.User);

            if (defaultRoleExists)
                await _userManager.AddToRoleAsync(user, DefaultRoleNames.User);

            if (Convert.ToBoolean(_configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false"))
            {
                try
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    string callbackUrl = string.Format("{0}/Account/ConfirmEmail/{1}?token={2}", _configuration["BlazorBoilerplate:ApplicationUrl"], user.Id, token);

                    var email = new EmailMessageDto();
                    email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                    email = EmailTemplates.BuildNewUserConfirmationEmail(email, user.UserName, user.Email, callbackUrl, user.Id.ToString(), token); //Replace First UserName with Name if you want to add name to Registration Form

                    _logger.LogInformation("New user created: {0}", user);
                    await _emailManager.SendEmailAsync(email);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"New user email failed: {ex.GetBaseException().Message}");
                }

                return new ApiResponse(Status200OK, "Create User Success");
            }

            try
            {
                var email = new EmailMessageDto();
                email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));
                email.BuildNewUserEmail(user.FullName, user.UserName, user.Email, parameters.Password);

                _logger.LogInformation("New user created: {0}", user);
                await _emailManager.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"New user email failed: {ex.GetBaseException().Message}");
            }

            var userInfo = new UserInfo
            {
                UserId = user.Id,
                IsAuthenticated = false,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            if (defaultRoleExists)
                userInfo.Roles = new List<string> { DefaultRoleNames.User };

            return new ApiResponse(Status200OK, L["User {0} created", userInfo.UserName], userInfo);
        }
        public async Task<ApiResponse> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning(L["The user {0} doesn't exist", id]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }
            if (user.UserName.ToLower() != DefaultUserNames.Administrator)
            {
                //TODO it could generate time-out
                //await _userProfileStore.DeleteAllApiLogsForUser(user.Id);

                await _userManager.DeleteAsync(user);
                return new ApiResponse(Status200OK, "User Deletion Successful");
            }
            else
                return new ApiResponse(Status403Forbidden, L["User {0} cannot be edited", user.UserName]);
        }
        public ApiResponse GetUser(ClaimsPrincipal user)
        {
            UserInfo userInfo = user != null && user.Identity.IsAuthenticated
                ? new UserInfo { UserName = user.Identity.Name, IsAuthenticated = true }
                : LoggedOutUser;
            return new ApiResponse(Status200OK, L["Operation Successful"], userInfo);
        }

        public async Task<ApiResponse> ListRoles()
        {
            return new ApiResponse(Status200OK, L["Operation Successful"], await _roleManager.Roles.Select(x => x.Name).ToListAsync());
        }

        public async Task<ApiResponse> Update(UserInfo userInfo)
        {
            var appUser = await _userManager.FindByIdAsync(userInfo.UserId.ToString());

            if (appUser.UserName.ToLower() != DefaultUserNames.Administrator && userInfo.UserName.ToLower() != DefaultUserNames.Administrator)
                appUser.UserName = userInfo.UserName;

            appUser.FirstName = userInfo.FirstName;
            appUser.LastName = userInfo.LastName;
            appUser.Email = userInfo.Email;

            try
            {
                await _userManager.UpdateAsync(appUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Updating user exception: {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, "Error Updating User");
            }

            if (userInfo.Roles != null)
            {
                try
                {
                    var rolesToAdd = new List<string>();
                    var currentUserRoles = (List<string>)await _userManager.GetRolesAsync(appUser);
                    foreach (var newUserRole in userInfo.Roles)
                    {
                        if (!currentUserRoles.Contains(newUserRole))
                        {
                            rolesToAdd.Add(newUserRole);
                        }
                    }
                    await _userManager.AddToRolesAsync(appUser, rolesToAdd);
                    //HACK to switch to claims auth
                    foreach (var role in rolesToAdd)
                    {
                        await _userManager.AddClaimAsync(appUser, new Claim($"Is{role}", "true"));
                    }

                    var rolesToRemove = currentUserRoles
                        .Where(role => !userInfo.Roles.Contains(role)).ToList();

                    if (appUser.UserName.ToLower() == DefaultUserNames.Administrator)
                        rolesToRemove.Remove(DefaultUserNames.Administrator);

                    await _userManager.RemoveFromRolesAsync(appUser, rolesToRemove);

                    //HACK to switch to claims auth
                    foreach (var role in rolesToRemove)
                    {
                        await _userManager.RemoveClaimAsync(appUser, new Claim($"Is{role}", "true"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Updating Roles exception: {ex.GetBaseException().Message}");
                    return new ApiResponse(Status500InternalServerError, "Error Updating Roles");
                }
            }

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        public async Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, string newPassword, ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning(L["The user {0} doesn't exist", id]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }
            var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, passToken, newPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation(user.UserName + "'s password reset; Requested from Admin interface by:" + userClaimsPrincipal.Identity.Name);
                return new ApiResponse(Status204NoContent, user.UserName + " password reset");
            }
            else
            {
                _logger.LogWarning(user.UserName + "'s password reset failed; Requested from Admin interface by:" + userClaimsPrincipal.Identity.Name);

                // this is going to an authenticated Admin so it should be safe/useful to send back raw error messages
                if (result.Errors.Any())
                {
                    var msg = string.Join(",", result.Errors.Select(i => i.Description));
                    _logger.LogWarning($"Error while resetting password of {user.UserName}: {msg}");
                    return new ApiResponse(Status400BadRequest, msg);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public async Task<ApplicationUser> RegisterNewUserAsync(string userName, string email, string password, bool requireConfirmEmail)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email
            };

            var result = password == null ?
                await _userManager.CreateAsync(user) :
                await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new DomainException(string.Join(",", result.Errors.Select(i => i.Description)));

            await _userManager.AddClaimsAsync(user, new Claim[]{
                    new Claim(Policies.IsUser, string.Empty),
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                });

            if (await _roleManager.RoleExistsAsync(DefaultRoleNames.User))
                await _userManager.AddToRoleAsync(user, DefaultRoleNames.User);

            _logger.LogInformation("New user registered: {0}", user);

            var emailMessage = new EmailMessageDto();

            if (requireConfirmEmail)
            {
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = $"{_configuration["BlazorBoilerplate:ApplicationUrl"]}/Account/ConfirmEmail/{user.Id}?token={token}";

                emailMessage.BuildNewUserConfirmationEmail(user.UserName, user.Email, callbackUrl, user.Id.ToString(), token); //Replace First UserName with Name if you want to add name to Registration Form
            }
            else
            {
                emailMessage.BuildNewUserEmail(user.FullName, user.UserName, user.Email, password);
            }

            emailMessage.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));

            try
            {
                await _emailManager.SendEmailAsync(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("New user email failed: Body: {0}, Error: {1}", emailMessage.Body, ex.GetBaseException().Message);
            }

            return user;
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;

            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToUpperInvariant();
        }

        private async Task<UserInfo> BuildUserInfo(ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.GetUserAsync(userClaimsPrincipal);

            if (user != null)
            {
                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

                var userInfo = new UserInfo
                {
                    IsAuthenticated = userClaimsPrincipal.Identity.IsAuthenticated,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserId = user.Id,
                    HasPassword = await _userManager.HasPasswordAsync(user),
                    PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                    TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                    HasAuthenticator = !string.IsNullOrEmpty(unformattedKey),
                    BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                    CountRecoveryCodes = await _userManager.CountRecoveryCodesAsync(user),

                    Logins = (await _userManager.GetLoginsAsync(user)).Select(i => new KeyValuePair<string, string>(i.LoginProvider, i.ProviderKey)).ToList(),

                    ExposedClaims = userClaimsPrincipal.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList(),
                    Roles = ((ClaimsIdentity)userClaimsPrincipal.Identity).Claims
                            .Where(c => c.Type == "role")
                            .Select(c => c.Value).ToList()
                };

                if (!userInfo.TwoFactorEnabled)
                {
                    if (!userInfo.HasAuthenticator)
                    {
                        await _userManager.ResetAuthenticatorKeyAsync(user);
                        unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                    }

                    userInfo.SharedKey = FormatKey(unformattedKey);
                    userInfo.AuthenticatorUri = string.Format(
                        "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                        _urlEncoder.Encode("BlazorBoilerplate"),
                        _urlEncoder.Encode(user.Email),
                        unformattedKey);
                }

                return userInfo;
            }
            else
            {
                return new UserInfo();
            }
        }
    }
}
