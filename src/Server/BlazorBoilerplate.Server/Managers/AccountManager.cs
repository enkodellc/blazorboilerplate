using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.DataModels;
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
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Server.Extensions;

namespace BlazorBoilerplate.Server.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountManager> _logger;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailManager _emailManager;
        private readonly IUserProfileStore _userProfileStore;
        private readonly IClientStore _clientStore;
        private readonly IConfiguration _configuration;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IStringLocalizer<Strings> L;

        private static readonly UserInfoDto LoggedOutUser = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };

        public AccountManager(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountManager> logger,
            RoleManager<IdentityRole<Guid>> roleManager,
            IEmailManager emailManager,
            IUserProfileStore userProfileStore,
            IClientStore clientStore,
            IConfiguration configuration,
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IStringLocalizer<Strings> l)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailManager = emailManager;
            _userProfileStore = userProfileStore;
            _clientStore = clientStore;
            _configuration = configuration;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
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
                _logger.LogInformation("User Email Confirmation Failed: {0}", string.Join(",", result.Errors.Select(i => i.Description)));
                return new ApiResponse(Status400BadRequest, L["EmailVerificationFailed"]);
            }

            await _signInManager.SignInAsync(user, true);

            return new ApiResponse(Status200OK, L["EmailVerificationSuccessful"]);
        }

        public async Task<ApiResponse> ForgotPassword(ForgotPasswordDto parameters)
        {
            var user = await _userManager.FindByEmailAsync(parameters.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
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
                _logger.LogError("Forgot Password email failed: {0}", ex.Message);
            }

            #endregion Forgot Password Email

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        public async Task<ApiResponse> BuildLoginViewModel(string returnUrl)
        {
            try
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
                        UserName = context?.LoginHint,
                    };

                    if (!local)
                    {
                        vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                    }

                    return new ApiResponse(Status200OK, "Success", vm);
                }

                var schemes = await _schemeProvider.GetAllSchemesAsync();

                var providers = schemes
                    .Where(x => x.DisplayName != null ||
                                (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
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

                return new ApiResponse(Status200OK, "Success", new LoginViewModel
                {
                    AllowRememberLogin = AccountOptions.AllowRememberLogin,
                    EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                    ReturnUrl = returnUrl,
                    UserName = context?.LoginHint,
                    ExternalProviders = providers.ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("BuildLoginViewModel Failed: " + ex.GetBaseException().Message);
                return new ApiResponse(Status500InternalServerError, "BuildLoginViewModel Failed");
            }
        }

        public async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(ClaimsPrincipal userClaimsPrincipal, HttpContext httpContext, string logoutId)
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

            if (userClaimsPrincipal?.Identity.IsAuthenticated == true)
            {
                var idp = userClaimsPrincipal.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
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
                var context = await _interaction.GetAuthorizationContextAsync(parameters.ReturnUrl);

                var result = await _signInManager.PasswordSignInAsync(parameters.UserName, parameters.Password, parameters.RememberMe, true);

                // If lock out activated and the max. amounts of attempts is reached.
                if (result.IsLockedOut)
                {
                    _logger.LogInformation("User Locked out: {0}", parameters.UserName);
                    return new ApiResponse(Status401Unauthorized, "User is locked out!");
                }

                // If your email is not confirmed but you require it in the settings for login.
                if (result.IsNotAllowed)
                {
                    _logger.LogInformation("User not allowed to log in: {0}", parameters.UserName);
                    return new ApiResponse(Status401Unauthorized, "Login not allowed!");
                }

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(parameters.UserName);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client?.ClientId));
                    _logger.LogInformation("Logged In: {0}", parameters.UserName);

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

                    if (string.IsNullOrEmpty(parameters.ReturnUrl))
                        parameters.ReturnUrl = await _userProfileStore.GetLastPageVisited(parameters.UserName);
                    else if (!parameters.IsValidReturnUrl)
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");

                    return new ApiResponse(Status200OK, parameters.ReturnUrl);
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(parameters.UserName, "Invalid Password for user {0}}", clientId: context?.Client.ClientId));
                _logger.LogInformation("Invalid Password for user {0}", parameters.UserName);
                return new ApiResponse(Status401Unauthorized, "Login Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError("Login Failed: " + ex.GetBaseException().Message);
                return new ApiResponse(Status500InternalServerError, "Login Failed");
            }
        }

        public async Task<ApiResponse> Logout(ClaimsPrincipal userClaimsPrincipal)
        {
            if (userClaimsPrincipal?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(userClaimsPrincipal.GetSubjectId(), userClaimsPrincipal.GetDisplayName()));
            }

            return new ApiResponse(Status200OK, "Logout Successful");
        }

        public async Task<ApiResponse> Register(RegisterDto parameters)
        {
            try
            {
                var requireConfirmEmail = Convert.ToBoolean(_configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false");

                await RegisterNewUserAsync(parameters.UserName, parameters.Email, parameters.Password, requireConfirmEmail);

                if (requireConfirmEmail)
                {
                    return new ApiResponse(Status200OK, "Register User Success");
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
            catch (DomainException ex)
            {
                _logger.LogError("Register User Failed: {0}, {1}", ex.Description, ex.Message);
                return new ApiResponse(Status400BadRequest, $"Register User Failed: {ex.Description} ");
            }
            catch (Exception ex)
            {
                _logger.LogError("Register User Failed: {0}", ex.Message);
                return new ApiResponse(Status400BadRequest, "Register User Failed");
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

            try
            {
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
                    _logger.LogWarning($"Error while resetting the password!: {user.UserName}");
                    return new ApiResponse(Status400BadRequest, $"Error while resetting the password!: {user.UserName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset Password failed: {ex.Message}");
                return new ApiResponse(Status400BadRequest, $"Error while resetting the password!: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UserInfo(ClaimsPrincipal userClaimsPrincipal)
        {
            var userInfo = await BuildUserInfo(userClaimsPrincipal);
            return new ApiResponse(Status200OK, "Retrieved UserInfo", userInfo);
        }

        public async Task<ApiResponse> UpdateUser(UserInfoDto userInfo)
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
                _logger.LogWarning("User Update Failed: {0}", string.Join(",", result.Errors.Select(i => i.Description)));
                return new ApiResponse(Status400BadRequest, "User Update Failed");
            }

            return new ApiResponse(Status200OK, "User Updated Successfully");
        }

        public async Task<ApiResponse> Create(RegisterDto parameters)
        {
            try
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
                    return new ApiResponse(Status400BadRequest, "Register User Failed: " + string.Join(",", result.Errors.Select(i => i.Description)));
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

                //Role - Here we tie the new user to the DefaultRoleNames.User role
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
                        _logger.LogError("New user email failed: {0}", ex.Message);
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
                    _logger.LogError("New user email failed: {0}", ex.Message);
                }

                var userInfo = new UserInfoDto
                {
                    UserId = user.Id,
                    IsAuthenticated = false,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    //ExposedClaims = user.Claims.ToDictionary(c => c.Type, c => c.Value),
                    Roles = new List<string> { DefaultRoleNames.User }
                };

                return new ApiResponse(Status200OK, L["User {0} created", userInfo.UserName], userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create User Failed: {ex.Message}");
                return new ApiResponse(Status400BadRequest, "Create User Failed");
            }
        }

        public async Task<ApiResponse> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning(L["The user {0} doesn't exist", id]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }
            try
            {

                //EF: not a fan this will delete old ApiLogs
                await _userProfileStore.DeleteAllApiLogsForUser(user.Id);

                await _userManager.DeleteAsync(user);
                return new ApiResponse(Status200OK, "User Deletion Successful");
            }
            catch
            {
                return new ApiResponse(Status400BadRequest, "User Deletion Failed");
            }
        }

        public ApiResponse GetUser(ClaimsPrincipal userClaimsPrincipal)
        {
            UserInfoDto userInfo = userClaimsPrincipal != null && userClaimsPrincipal.Identity.IsAuthenticated
                ? new UserInfoDto { UserName = userClaimsPrincipal.Identity.Name, IsAuthenticated = true }
                : LoggedOutUser;
            return new ApiResponse(Status200OK, "Get User Successful", userInfo);
        }

        public async Task<ApiResponse> ListRoles()
        {
            return new ApiResponse(Status200OK, "", await _roleManager.Roles.Select(x => x.Name).ToListAsync());
        }

        public async Task<ApiResponse> Update(UserInfoDto userInfo)
        {
            // retrieve full user object for updating
            var appUser = await _userManager.FindByIdAsync(userInfo.UserId.ToString()).ConfigureAwait(true);

            //update values
            appUser.UserName = userInfo.UserName;
            appUser.FirstName = userInfo.FirstName;
            appUser.LastName = userInfo.LastName;
            appUser.Email = userInfo.Email;

            try
            {
                await _userManager.UpdateAsync(appUser).ConfigureAwait(true);
            }
            catch
            {
                return new ApiResponse(Status500InternalServerError, "Error Updating User");
            }

            if (userInfo.Roles != null)
            {
                try
                {
                    var rolesToAdd = new List<string>();
                    var currentUserRoles = (List<string>)(await _userManager.GetRolesAsync(appUser).ConfigureAwait(true));
                    foreach (var newUserRole in userInfo.Roles)
                    {
                        if (!currentUserRoles.Contains(newUserRole))
                        {
                            rolesToAdd.Add(newUserRole);
                        }
                    }
                    await _userManager.AddToRolesAsync(appUser, rolesToAdd).ConfigureAwait(true);
                    //HACK to switch to claims auth
                    foreach (var role in rolesToAdd)
                    {
                        await _userManager.AddClaimAsync(appUser, new Claim($"Is{role}", "true")).ConfigureAwait(true);
                    }

                    var rolesToRemove = currentUserRoles
                        .Where(role => !userInfo.Roles.Contains(role)).ToList();

                    await _userManager.RemoveFromRolesAsync(appUser, rolesToRemove).ConfigureAwait(true);

                    //HACK to switch to claims auth
                    foreach (var role in rolesToRemove)
                    {
                        await _userManager.RemoveClaimAsync(appUser, new Claim($"Is{role}", "true")).ConfigureAwait(true);
                    }
                }
                catch
                {
                    return new ApiResponse(Status500InternalServerError, "Error Updating Roles");
                }
            }
            return new ApiResponse(Status200OK, "User Updated");
        }

        public async Task<ApiResponse> AdminResetUserPasswordAsync(Guid id, string newPassword, ClaimsPrincipal userClaimsPrincipal)
        {
            ApplicationUser user;

            try
            {
                user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    throw new KeyNotFoundException();
                }
            }
            catch (KeyNotFoundException ex)
            {
                return new ApiResponse(Status400BadRequest, "Unable to find user" + ex.Message);
            }
            try
            {
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
                        return new ApiResponse(Status400BadRequest, string.Join(',', result.Errors.Select(x => x.Description)));
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception ex) // not sure if failed password reset result will throw an exception
            {
                _logger.LogError(user.UserName + "'s password reset failed; Requested from Admin interface by:" + userClaimsPrincipal.Identity.Name);
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }

        public async Task<ApplicationUser> RegisterNewUserAsync(string userName, string email, string password, bool requireConfirmEmail)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email
            };

            var createUserResult = password == null ?
                await _userManager.CreateAsync(user) :
                await _userManager.CreateAsync(user, password);

            if (!createUserResult.Succeeded)
                throw new DomainException(string.Join(",", createUserResult.Errors.Select(i => i.Description)));

            await _userManager.AddClaimsAsync(user, new Claim[]{
                    new Claim(Policies.IsUser, string.Empty),
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                });

            //Role - Here we tie the new user to the DefaultRoleNames.User role
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
                _logger.LogError("New user email failed: Body: {0}, Error: {1}", emailMessage.Body, ex.Message);
            }

            return user;
        }

        private async Task<UserInfoDto> BuildUserInfo(ClaimsPrincipal userClaimsPrincipal)
        {
            var user = await _userManager.GetUserAsync(userClaimsPrincipal);

            if (user != null)
            {
                try
                {
                    return new UserInfoDto
                    {
                        IsAuthenticated = userClaimsPrincipal.Identity.IsAuthenticated,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserId = user.Id,
                        //Optionally: filter the claims you want to expose to the client
                        ExposedClaims = userClaimsPrincipal.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList(),
                        Roles = ((ClaimsIdentity)userClaimsPrincipal.Identity).Claims
                                .Where(c => c.Type == "role")
                                .Select(c => c.Value).ToList()
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not build UserInfoDto: " + ex.Message);
                }
            }
            else
            {
                return new UserInfoDto();
            }

            return null;
        }
    }
}
