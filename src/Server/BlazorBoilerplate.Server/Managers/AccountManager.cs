using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Storage;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    [ApiResponseException]
    public class AccountManager : IAccountManager
    {
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountManager> _logger;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailFactory _emailFactory;
        private readonly IEmailManager _emailManager;
        private readonly IClientStore _clientStore;
        private readonly ApplicationDbContext _dbContext;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly UrlEncoder _urlEncoder;
        private readonly IEventService _events;
        private readonly IStringLocalizer<Global> L;
        private readonly string baseUrl;

        private static readonly UserViewModel LoggedOutUser = new() { IsAuthenticated = false, Roles = new List<string>() };

        public AccountManager(IDatabaseInitializer databaseInitializer,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountManager> logger,
            RoleManager<ApplicationRole> roleManager,
            IEmailFactory emailFactory,
            IEmailManager emailManager,
            IClientStore clientStore,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            UrlEncoder urlEncoder,
            IEventService events,
            IStringLocalizer<Global> l)
        {
            _databaseInitializer = databaseInitializer;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailFactory = emailFactory;
            _emailManager = emailManager;
            _clientStore = clientStore;
            _dbContext = dbContext;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _urlEncoder = urlEncoder;
            _events = events;
            L = l;
            baseUrl = configuration[$"{nameof(BlazorBoilerplate)}:ApplicationUrl"];
        }

        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailViewModel parameters)
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

            if (!user.EmailConfirmed)
            {
                var token = parameters.Token;
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                {
                    var msg = result.GetErrors();
                    _logger.LogWarning("User Email Confirmation Failed: {0}", msg);
                    return new ApiResponse(Status400BadRequest, msg);
                }

                await _userManager.RemoveClaimAsync(user, new Claim(JwtClaimTypes.EmailVerified, ClaimValues.falseString, ClaimValueTypes.Boolean));
                await _userManager.AddClaimAsync(user, new Claim(JwtClaimTypes.EmailVerified, ClaimValues.trueString, ClaimValueTypes.Boolean));
            }

            return new ApiResponse(Status200OK, L["EmailVerificationSuccessful"]);
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

        public async Task<LoggedOutViewModel> BuildLoggedOutViewModel(ClaimsPrincipal authenticatedUser, HttpContext httpContext, string logoutId)
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

            if (authenticatedUser?.Identity.IsAuthenticated == true)
            {
                var idp = authenticatedUser.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
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
                await _databaseInitializer.EnsureAdminIdentities();

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
                    var normalizeUserName = _userManager.NormalizeName(parameters.UserName);
                    //AdditionalUserClaimsPrincipalFactory needs Person to add extra claims
                    var user = await _dbContext.Users.Include(i => i.Person).SingleAsync(i => i.NormalizedUserName == normalizeUserName);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client?.ClientId));
                    _logger.LogInformation("Logged In user {0}", parameters.UserName);

                    await SetCultureInProfile(user.Id);

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
        public async Task<ApiResponse> Logout(ClaimsPrincipal authenticatedUser)
        {
            if (authenticatedUser?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(authenticatedUser.GetSubjectId(), authenticatedUser.GetDisplayName()));
            }

            return new ApiResponse(Status200OK, "Logout Successful");
        }

        public async Task<ApiResponse> Register(RegisterViewModel parameters)
        {
            await RegisterNewUser(parameters.UserName, parameters.Email, parameters.Password, false);

            if (_userManager.Options.SignIn.RequireConfirmedEmail)
                return new ApiResponse(Status200OK, L["Operation Successful"]);
            else
            {
                return await Login(new LoginInputModel
                {
                    UserName = parameters.UserName,
                    Password = parameters.Password
                });
            }
        }

        #region password
        public async Task<ApiResponse> ForgotPassword(ForgotPasswordViewModel parameters)
        {
            var user = await _userManager.FindByEmailAsync(parameters.Email);

            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogInformation("Forgot Password with non-existent email / user: {0}", parameters.Email);
                // Don't reveal that the user does not exist or is not confirmed
                return new ApiResponse(Status200OK, L["Operation Successful"]);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string callbackUrl = string.Format("{0}/Account/ResetPassword/{1}?token={2}", baseUrl, user.Id, token); //token must be a query string parameter as it is very long

            var email = _emailFactory.BuildForgotPasswordEmail(user.UserName, callbackUrl, token);
            email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));

            var response = await _emailManager.QueueEmail(email, EmailType.Password);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation($"Reset Password Successful Email Sent: {user.Email}");
            else
                _logger.LogError($"Reset Password Successful Email Sent: {user.Email}");

            return response;
        }
        public async Task<ApiResponse> ResetPassword(ResetPasswordViewModel parameters)
        {
            var user = await _userManager.FindByIdAsync(parameters.UserId);

            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", parameters.UserId]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.ResetPasswordAsync(user, parameters.Token, parameters.Password);

            if (result.Succeeded)
            {
                var email = _emailFactory.BuildPasswordResetEmail(user.UserName);
                email.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));

                var response = await _emailManager.QueueEmail(email, EmailType.Password);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation($"Reset Password Successful Email to {user.Email}");
                else
                    _logger.LogError($"Fail to send Reset Password Email to {user.Email}");

                return response;
            }
            else
            {
                var msg = result.GetErrors();
                _logger.LogWarning("Error while resetting the password: {0}", msg);
                return new ApiResponse(Status400BadRequest, msg);
            }
        }
        public async Task<ApiResponse> UpdatePassword(ClaimsPrincipal authenticatedUser, UpdatePasswordViewModel parameters)
        {
            var user = await _userManager.FindByIdAsync(authenticatedUser.GetSubjectId());

            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", authenticatedUser.GetDisplayName()]);
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
                var msg = result.GetErrors();
                _logger.LogWarning($"Error while updating the password of {user.UserName}: {msg}");
                return new ApiResponse(Status400BadRequest, msg);
            }
        }
        #endregion

        #region 2fa
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

                //AdditionalUserClaimsPrincipalFactory needs Person to add extra claims
                await _dbContext.Entry(user).Reference(i => i.Person).LoadAsync();

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

                    await SetCultureInProfile(user.Id);

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

                    await SetCultureInProfile(user.Id);

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
        public async Task<ApiResponse> EnableAuthenticator(ClaimsPrincipal authenticatedUser, AuthenticatorVerificationCodeViewModel parameters)
        {
            var user = await _userManager.FindByIdAsync(authenticatedUser.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", authenticatedUser.GetDisplayName()]);
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

                    var userViewModel = await BuildUserViewModel(authenticatedUser);

                    if (await _userManager.CountRecoveryCodesAsync(user) == 0)
                    {
                        userViewModel.RecoveryCodes = (await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).ToArray();
                    }

                    return new ApiResponse(Status200OK, L["Operation Successful"], userViewModel);
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
        public async Task<ApiResponse> DisableAuthenticator(ClaimsPrincipal authenticatedUser)
        {
            var user = await _userManager.FindByIdAsync(authenticatedUser.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", authenticatedUser.GetDisplayName()]);
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

            return new ApiResponse(Status200OK, L["Operation Successful"], await BuildUserViewModel(authenticatedUser));
        }
        public async Task<ApiResponse> ForgetTwoFactorClient(ClaimsPrincipal authenticatedUser)
        {
            var user = await _userManager.FindByIdAsync(authenticatedUser.GetSubjectId());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", authenticatedUser.GetDisplayName()]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            await _signInManager.ForgetTwoFactorClientAsync();

            return new ApiResponse(Status200OK, L["Operation Successful"], await BuildUserViewModel(authenticatedUser));
        }

        public async Task<ApiResponse> Enable2fa(Guid userId, ClaimsPrincipal authenticatedUser = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userId]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);

            if (result.Succeeded)
            {
                return new ApiResponse(Status200OK, "Enabling 2fa Successful", authenticatedUser != null ? await BuildUserViewModel(authenticatedUser) : await BuildUserViewModel(userId));
            }
            else
                return new ApiResponse(Status400BadRequest, "Error while enabling 2fa");
        }
        public async Task<ApiResponse> Disable2fa(Guid userId, ClaimsPrincipal authenticatedUser = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogInformation(L["The user {0} doesn't exist", userId]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

            if (result.Succeeded)
            {
                return new ApiResponse(Status200OK, "Disabling 2fa Successful", authenticatedUser != null ? await BuildUserViewModel(authenticatedUser) : await BuildUserViewModel(userId));
            }
            else
                return new ApiResponse(Status400BadRequest, "Error while disabling 2fa");
        }
        #endregion

        public async Task<ApiResponse> UserViewModel(ClaimsPrincipal authenticatedUser)
        {
            var userViewModel = await BuildUserViewModel(authenticatedUser);
            return new ApiResponse(Status200OK, L["Operation Successful"], userViewModel);
        }

        public async Task<ApiResponse> UserViewModel(Guid id)
        {
            var userViewModel = await BuildUserViewModel(id);
            return new ApiResponse(Status200OK, L["Operation Successful"], userViewModel);
        }

        private async Task<ApiResponse> UpdateClaim(ApplicationUser user, IEnumerable<Claim> claims, string claimType, bool updateClaimValue, ClaimsPrincipal authenticatedUser)
        {
            var currentClaimValue = claims.Any(claim => claim.Type == claimType && claim.Value == ClaimValues.trueString);

            if (currentClaimValue && !updateClaimValue)
            {
                var result = await _userManager.RemoveClaimAsync(user, new Claim(claimType, ClaimValues.trueString));

                if (!result.Succeeded)
                {
                    var msg = result.GetErrors();
                    _logger.LogWarning($"Removing claim {claimType} to {user.UserName} by {authenticatedUser.Identity.Name} failed: {msg}");
                    return new ApiResponse(Status400BadRequest, msg);
                }
                else
                    _logger.LogInformation($"Claim {claimType} removed from {user.UserName} by {authenticatedUser.Identity.Name}");
            }
            else if (!currentClaimValue && updateClaimValue)
            {
                var result = await _userManager.AddClaimAsync(user, new Claim(claimType, ClaimValues.trueString));

                if (!result.Succeeded)
                {
                    var msg = result.GetErrors();
                    _logger.LogWarning($"Adding claim {claimType} to {user.UserName} by {authenticatedUser.Identity.Name} failed: {msg}");

                    return new ApiResponse(Status400BadRequest, msg);
                }
                else
                    _logger.LogInformation($"Claim {claimType} added to {user.UserName} by {authenticatedUser.Identity.Name}");
            }

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        private async Task AddToRole(ApplicationUser user, string role)
        {
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    var msg = string.Join(",", result.Errors.Select(i => i.Description));

                    _logger.LogWarning("AddToRoleAsync Failed: {0}", msg);

                    throw new DomainException(msg);
                }
            }
        }

        private async Task RemoveFromRole(ApplicationUser user, string role)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    var msg = string.Join(",", result.Errors.Select(i => i.Description));

                    _logger.LogWarning("AddToRoleAsync Failed: {0}", msg);

                    throw new DomainException(msg);
                }
            }
        }

        private async Task<ApiResponse> UpdateManagerPermissions(ApplicationUser user, UserViewModel model, ClaimsPrincipal authenticatedUser)
        {
            if (user != null)
            {
                var claims = (await _userManager.GetClaimsAsync(user)).ToArray();

                foreach (var userFeature in Enum.GetValues<UserFeatures>())
                {
                    var result = await UpdateClaim(user, claims, ApplicationClaimTypes.For(userFeature), model.UserFeatures[userFeature], authenticatedUser);

                    if (!result.IsSuccessStatusCode)
                        return result;

                    if (model.UserFeatures[userFeature])
                        await AddToRole(user, userFeature.ToString());
                    else
                        await RemoveFromRole(user, userFeature.ToString());
                }

                return new ApiResponse(Status200OK, L["Operation Successful"]);
            }
            else
                return new ApiResponse(Status400BadRequest, L["InvalidData"]);
        }
        public async Task<ApiResponse> UpdateUser(UserViewModel userViewModel, bool isUpsert, ClaimsPrincipal authenticatedUser)
        {
            ApplicationUser user;

            if (userViewModel.UserId != null)
            {
                //var email = _userManager.NormalizeEmail(userViewModel.Email);

                user = await _userManager.Users
                    .Include(i => i.Person).ThenInclude(i => i.Company)
                    .Include(i => i.Person).ThenInclude(i => i.CreatedBy)
                    .SingleAsync(i => i.Id == userViewModel.UserId);

                if (user == null)
                {
                    _logger.LogWarning("userViewModel.UserId not found");
                    return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
                }
                else
                {
                    user.UserName = userViewModel.UserName;
                    user.Email = userViewModel.Email;
                }
            }
            else
            {
                if (isUpsert)
                {
                    user = new ApplicationUser
                    {
                        UserName = userViewModel.UserName,
                        Email = userViewModel.Email
                    };

                    await RegisterNewUser(user, userViewModel.Password, true);
                }
                else
                {
                    _logger.LogWarning("userViewModel.UserId is null");
                    return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
                }
            }

            if (user.Person == null &&
                (!string.IsNullOrWhiteSpace(userViewModel.FirstName) || !string.IsNullOrWhiteSpace(userViewModel.LastName) || !string.IsNullOrWhiteSpace(userViewModel.CompanyName)))
                user.Person = new Person();

            user.Person.FirstName = userViewModel.FirstName;
            user.Person.LastName = userViewModel.LastName;

            if (userViewModel.ExpirationDate != user.Person.ExpirationDate)
            {
                user.Person.ExpirationDate = userViewModel.ExpirationDate;
                user.Person.ExpirationReminderSentOn = null;
            }

            var company = await _dbContext.Companies.SingleOrDefaultAsync(i => i.VatIn == userViewModel.CompanyVatIn);

            if (user.Person.Company == null && !string.IsNullOrWhiteSpace(userViewModel.CompanyName))
                user.Person.Company = company ?? new Company();

            if (user.Person.Company != null)
            {
                user.Person.Company.Name = userViewModel.CompanyName;
                user.Person.Company.Longitude = userViewModel.CompanyLongitude;
                user.Person.Company.Latitude = userViewModel.CompanyLatitude;
                user.Person.Company.Address = userViewModel.CompanyAddress;
                user.Person.Company.City = userViewModel.CompanyCity;
                user.Person.Company.Province = userViewModel.CompanyProvince;
                user.Person.Company.ZipCode = userViewModel.CompanyZipCode;
                user.Person.Company.CountryCode = userViewModel.CompanyCountryCode;
                user.Person.Company.VatIn = userViewModel.CompanyVatIn;
                user.Person.Company.PhoneNumber = userViewModel.CompanyPhoneNumber;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var msg = result.GetErrors();
                _logger.LogWarning("User Update Failed: {0}", msg);
                return new ApiResponse(Status400BadRequest, msg);
            }

            var updateManagerPermissionsResult = await UpdateManagerPermissions(user, userViewModel, authenticatedUser);

            if (!updateManagerPermissionsResult.IsSuccessStatusCode)
            {
                return updateManagerPermissionsResult;
            }

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        public async Task<ApplicationUser> RegisterNewUser(string userName, string email, string password, bool emailConfirmedByAdmin)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email
            };

            return await RegisterNewUser(user, password, emailConfirmedByAdmin);
        }
        private async Task<ApplicationUser> RegisterNewUser(ApplicationUser user, string password, bool emailConfirmedByAdmin)
        {
            var result = password == null ?
                await _userManager.CreateAsync(user) :
                await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new DomainException(result.GetErrors());

            user.EmailConfirmed = emailConfirmedByAdmin;

            await _userManager.AddClaimsAsync(user, new Claim[]{
                    new Claim(Policies.IsUser, string.Empty),
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.EmailVerified, emailConfirmedByAdmin? ClaimValues.trueString : ClaimValues.falseString, ClaimValueTypes.Boolean)
                });

            _logger.LogInformation("New user registered: {0}", user);

            EmailMessageDto emailMessage;

            var requireConfirmedEmail = !emailConfirmedByAdmin && _userManager.Options.SignIn.RequireConfirmedEmail;

            if (requireConfirmedEmail)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = $"{baseUrl}/Account/ConfirmEmail/{user.Id}?token={token}";

                emailMessage = _emailFactory.BuildNewUserConfirmationEmail(user.Person?.FullName, user.UserName, callbackUrl);
            }
            else
            {
                emailMessage = _emailFactory.BuildNewUserEmail(user.Person?.FullName, user.UserName, user.Email, password);
            }

            emailMessage.ToAddresses.Add(new EmailAddressDto(user.Email, user.Email));

            var response = requireConfirmedEmail ? await _emailManager.QueueEmail(emailMessage, EmailType.Confirmation) : await _emailManager.QueueEmail(emailMessage, EmailType.Password);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation($"New user email sent to {user.Email}");
            else
                _logger.LogError("New user email failed: Body: {0}, Error: {1}", emailMessage.Body, response.Message);

            return user;
        }

        //By Admin
        public async Task<ApiResponse> Create(RegisterViewModel parameters)
        {
            var user = new ApplicationUser
            {
                UserName = parameters.UserName,
                Email = parameters.Email
            };

            await RegisterNewUser(user, parameters.Password, true);

            var userViewModel = new UserViewModel
            {
                UserId = user.Id,
                IsAuthenticated = false,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.Person?.FirstName,
                LastName = user.Person?.LastName,
                ExpirationDate = user.Person?.ExpirationDate
            };

            return new ApiResponse(Status200OK, L["User {0} created", userViewModel.UserName], userViewModel);
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

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return new ApiResponse(Status200OK, L["Operation Successful"]);
                else
                {
                    var msg = result.GetErrors();
                    _logger.LogWarning("User delete failed: {0}", msg);
                    return new ApiResponse(Status400BadRequest, msg);
                }
            }
            else
                return new ApiResponse(Status403Forbidden, L["User {0} cannot be edited", user.UserName]);
        }
        public ApiResponse GetUser(ClaimsPrincipal authenticatedUser)
        {
            UserViewModel userViewModel = authenticatedUser != null && authenticatedUser.Identity.IsAuthenticated
                ? new UserViewModel { UserName = authenticatedUser.Identity.Name, IsAuthenticated = true }
                : LoggedOutUser;

            return new ApiResponse(Status200OK, L["Operation Successful"], userViewModel);
        }

        public async Task<ApiResponse> AdminUpdateUser(UserViewModel userViewModel)
        {
            var user = await _userManager.Users.Include(i => i.Person).SingleAsync(i => i.Id == userViewModel.UserId);

            if (user.UserName.ToLower() != DefaultUserNames.Administrator && userViewModel.UserName.ToLower() != DefaultUserNames.Administrator)
                user.UserName = userViewModel.UserName;

            if (user.Person == null)
                user.Person = new Person();

            user.Person.FirstName = userViewModel.FirstName;
            user.Person.LastName = userViewModel.LastName;
            user.Person.ExpirationDate = userViewModel.ExpirationDate;
            user.Email = userViewModel.Email;

            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Updating user exception: {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, L["Operation Failed"]);
            }

            if (userViewModel.Roles != null)
            {
                try
                {
                    var rolesToAdd = new List<string>();
                    var currentUserRoles = (List<string>)await _userManager.GetRolesAsync(user);

                    foreach (var newUserRole in userViewModel.Roles)
                    {
                        if (!currentUserRoles.Contains(newUserRole))
                            rolesToAdd.Add(newUserRole);
                    }

                    if (rolesToAdd.Count > 0)
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);

                        //HACK to switch to claims auth
                        foreach (var role in rolesToAdd)
                            await _userManager.AddClaimAsync(user, new Claim($"Is{role}", ClaimValues.trueString));
                    }

                    var rolesToRemove = currentUserRoles.Where(role => !userViewModel.Roles.Contains(role)).ToList();

                    if (rolesToRemove.Count > 0)
                    {
                        if (user.UserName.ToLower() == DefaultUserNames.Administrator)
                            rolesToRemove.Remove(DefaultUserNames.Administrator);

                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                        //HACK to switch to claims auth
                        foreach (var role in rolesToRemove)
                            await _userManager.RemoveClaimAsync(user, new Claim($"Is{role}", ClaimValues.trueString));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Updating Roles exception: {ex.GetBaseException().Message}");
                    return new ApiResponse(Status500InternalServerError, L["Operation Failed"]);
                }
            }

            return new ApiResponse(Status200OK, L["Operation Successful"]);
        }

        public async Task<ApiResponse> AdminResetUserPassword(ChangePasswordViewModel changePasswordViewModel, ClaimsPrincipal authenticatedUser)
        {
            var user = await _userManager.FindByIdAsync(changePasswordViewModel.UserId);

            if (user == null)
            {
                _logger.LogWarning(L["The user {0} doesn't exist", changePasswordViewModel.UserId]);
                return new ApiResponse(Status404NotFound, L["The user doesn't exist"]);
            }

            var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, passToken, changePasswordViewModel.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation(user.UserName + "'s password reset; Requested from Admin interface by:" + authenticatedUser.Identity.Name);
                return new ApiResponse(Status204NoContent, user.UserName + " password reset");
            }
            else
            {
                _logger.LogWarning(user.UserName + "'s password reset failed; Requested from Admin interface by:" + authenticatedUser.Identity.Name);

                var msg = result.GetErrors();
                _logger.LogWarning($"Error while resetting password of {user.UserName}: {msg}");
                return new ApiResponse(Status400BadRequest, msg);
            }
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;

            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey[currentPosition..]);
            }

            return result.ToString().ToUpperInvariant();
        }

        private async Task<UserViewModel> BuildUserViewModel(ClaimsPrincipal authenticatedUser)
        {
            var id = new Guid(authenticatedUser.Identity.GetSubjectId());

            var userViewModel = await BuildUserViewModel(id);

            if (userViewModel.UserName != null)
            {
                userViewModel.IsAuthenticated = authenticatedUser.Identity.IsAuthenticated;
                userViewModel.ExposedClaims = authenticatedUser.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList();
                userViewModel.Roles = ((ClaimsIdentity)authenticatedUser.Identity).Claims
                        .Where(c => c.Type == "role")
                        .Select(c => c.Value).ToList();
            }

            return userViewModel;
        }
        private async Task<UserViewModel> BuildUserViewModel(Guid id)
        {
            var user = await _userManager.Users.Include(i => i.Person).ThenInclude(i => i.Company).SingleOrDefaultAsync(i => i.Id == id); //user==null browser cache of deleted users

            if (user != null)
            {
                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

                var claims = await _userManager.GetClaimsAsync(user);

                var userViewModel = new UserViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    HasPassword = await _userManager.HasPasswordAsync(user),
                    PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                    TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                    HasAuthenticator = !string.IsNullOrEmpty(unformattedKey),
                    BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                    CountRecoveryCodes = await _userManager.CountRecoveryCodesAsync(user),

                    ExposedClaims = claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList(),
                    Roles = claims.Where(c => c.Type == "role").Select(c => c.Value).ToList(),

                    Logins = (await _userManager.GetLoginsAsync(user)).Select(i => new KeyValuePair<string, string>(i.LoginProvider, i.ProviderKey)).ToList()
                };

                if (user.Person is Person person)
                {
                    userViewModel.FirstName = person.FirstName;
                    userViewModel.LastName = person.LastName;
                    userViewModel.ExpirationDate = person.ExpirationDate;

                    foreach (var userFeature in Enum.GetValues<UserFeatures>())
                        userViewModel.UserFeatures[userFeature] = userViewModel.ExposedClaims.Any(claim => claim.Key == ApplicationClaimTypes.For(userFeature) && claim.Value == ClaimValues.trueString);

                    if (person.Company is Company company)
                    {
                        userViewModel.CompanyName = company.Name;
                        userViewModel.CompanyLongitude = company.Longitude;
                        userViewModel.CompanyLatitude = company.Latitude;
                        userViewModel.CompanyAddress = company.Address;
                        userViewModel.CompanyCity = company.City;
                        userViewModel.CompanyProvince = company.Province;
                        userViewModel.CompanyZipCode = company.ZipCode;
                        userViewModel.CompanyCountryCode = company.CountryCode;
                        userViewModel.CompanyVatIn = company.VatIn;
                        userViewModel.CompanyPhoneNumber = company.PhoneNumber;
                    }
                }

                if (!userViewModel.TwoFactorEnabled)
                {
                    if (!userViewModel.HasAuthenticator)
                    {
                        await _userManager.ResetAuthenticatorKeyAsync(user);
                        unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                    }

                    userViewModel.SharedKey = FormatKey(unformattedKey);
                    userViewModel.AuthenticatorUri = string.Format(
                        "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                        _urlEncoder.Encode(nameof(BlazorBoilerplate)),
                        _urlEncoder.Encode(user.Email),
                        unformattedKey);
                }

                return userViewModel;
            }
            else
            {
                return new UserViewModel();
            }
        }

        private async Task SetCultureInProfile(Guid id)
        {
            var profile = await _dbContext.UserProfiles.SingleOrDefaultAsync(i => i.UserId == id);

            if (profile == null)
            {
                profile = new UserProfile { UserId = id, LastUpdatedDate = DateTime.Now };
            }

            profile.Culture = CultureInfo.CurrentCulture.Name;

            await _dbContext.SaveChangesAsync();
        }
    }
}
