using AutoMapper;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Server.Factories;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware;
using BlazorBoilerplate.Server.Providers;
using BlazorBoilerplate.Shared.Dto.ExternalAuth;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.Providers; //ServerSideBlazor
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.Shared.Validators.Db;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Mapping;
using Breeze.AspNetCore;
using Breeze.Core;
using FluentValidation.AspNetCore;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization; //ServerSideBlazor
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using Serilog;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using static BlazorBoilerplate.Constants.PasswordPolicy;
using static IdentityModel.JwtClaimTypes;
using static IdentityServer4.IdentityServerConstants;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _environment;
        private readonly bool _enableAPIDoc;

        private readonly string projectName = nameof(BlazorBoilerplate);

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _environment = env;
            _enableAPIDoc = configuration.GetSection("BlazorBoilerplate:Api:Doc:Enabled").Get<bool>();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILocalizationProvider, StorageLocalizationProvider>();
            services.AddTextLocalization(options =>
            {
                options.ReturnOnlyKeyIfNotFound = !_environment.IsDevelopment();
                options.FallBackNeutralCulture = !_environment.IsDevelopment();
            }).Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(Settings.SupportedCultures[0]);
                options.AddSupportedCultures(Settings.SupportedCultures);
                options.AddSupportedUICultures(Settings.SupportedCultures);
            });

            services.AddScoped<LazyAssemblyLoader>();

            var dataProtectionBuilder = services.AddDataProtection().SetApplicationName(projectName);

            services.RegisterStorage(Configuration);

            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                AdditionalUserClaimsPrincipalFactory>();

            var authAuthority = Configuration[$"{projectName}:IS4ApplicationUrl"].TrimEnd('/');

            // Adds IdentityServer https://identityserver4.readthedocs.io/en/latest/reference/options.html
            var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = authAuthority;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.ErrorUrl = "/identityserver/error";
            })
              .AddIdentityServerStores(Configuration)
              .AddAspNetIdentity<ApplicationUser>(); //https://identityserver4.readthedocs.io/en/latest/reference/aspnet_identity.html

            X509Certificate2 cert = null;

            var keysLocalFolder = Path.Combine(_environment.ContentRootPath, "Keys");

            if (_environment.IsDevelopment())
            {
                // The AddDeveloperSigningCredential extension creates temporary key material tempkey.jwk for signing tokens.
                // This might be useful to get started, but needs to be replaced by some persistent key material for production scenarios.
                // See http://docs.identityserver.io/en/release/topics/crypto.html#refcrypto for more information.
                // https://stackoverflow.com/questions/42351274/identityserver4-hosting-in-iis

                identityServerBuilder.AddDeveloperSigningCredential();

                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysLocalFolder));
            }
            else
            {
                // Running on Azure Web App service - read the setup doc at blazor-boilerplate.readthedocs.io

                // appsettings.json parameters used:
                //  "RunsOnAzure": true,
                //  "RunningAsAppService": true,
                //  "RunningAsDocker": false, // not implemented yet
                //  "AzureKeyVault": {
                //      "UsingKeyVault": true,
                //      "UseManagedAppIdentity": true, 
                //      "AppKey": "", // not implemented yet.
                //      "AppSecret": "",
                //      "KeyVaultURI": "https://YOURVAULTNAMEHERE.vault.azure.net/",
                //      "CertificateIdentifier": "https://YOURVAULTNAMEHERE.vault.azure.net/certificates/BBAUTH/<HEX_VERSION_STRING_HERE>",
                //      "CertificateName": "BBAUTH", 
                //      "StorageAccountBlobBaseUrl": "https://<YOUR_STORAGE_ACCOUNT_NAME_HERE>.blob.core.windows.net",
                //      "ContainerName": "blazor-boilerplate-keys",
                //      "KeysBlobName": "keys.xml"
                if (Convert.ToBoolean(Configuration["HostingOnAzure:RunsOnAzure"]) == true)
                {
                    if (Convert.ToBoolean(Configuration["HostingOnAzure:AzureKeyVault:UsingKeyVault"]) == true)
                    {
                        if (Convert.ToBoolean(Configuration["HostingOnAzure:AzurekeyVault:UseManagedAppIdentity"]) == true)
                        {
                            //https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview

                            // In production environment we have already configured Managed Identity (blazor-boilerplate) using Azure Portal for our app to access Key Vault and Blob Storage. 

                            // Set up TokenCredential options for production and development environments
                            bool isDeployed = !_environment.IsDevelopment();
                            var credentialOptions = new DefaultAzureCredentialOptions
                            {
                                ExcludeEnvironmentCredential = isDeployed,
                                ExcludeManagedIdentityCredential = false, // we only use this one in production
                                ExcludeSharedTokenCacheCredential = isDeployed,
                                ExcludeVisualStudioCredential = isDeployed,
                                ExcludeVisualStudioCodeCredential = isDeployed,
                                ExcludeAzureCliCredential = isDeployed,
                                ExcludeInteractiveBrowserCredential = isDeployed,

                            };

                            // In development environment DefaultAzureCredential() will use the shared token credential from the IDE. In Visual Studio this is under Options - Azure Service Authentication.
                            if (_environment.IsDevelopment())
                            {
                                // credentialOptions.SharedTokenCacheUsername = "<username@abc.onmicrosoft.com>"; // specify user name to use if more than one Azure username is configured in IDE. 
                                // var defaultTenantId = "?????-????-????-????-????????"; // specify AAD tenant to authenticate against (from Azure Portal - AAD - Tenant ID) 
                                // credentialOptions.SharedTokenCacheTenantId = defaultTenantId;
                                // credentialOptions.VisualStudioCodeTenantId = defaultTenantId;
                                // credentialOptions.VisualStudioTenantId = defaultTenantId;
                            }

                            TokenCredential tokenCredential = new DefaultAzureCredential(credentialOptions);

                            // The Azure Storage Container (blazor-boilerplate-keys) Access Control must grant blazor-boilerplate the following roles:
                            // - Storage Blob Data Contributor

                            var blobServiceClient = new BlobServiceClient(
                                new Uri(Configuration["HostingOnAzure:AzureKeyVault:StorageAccountBlobBaseUrl"]),
                                tokenCredential);

                            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(Configuration["HostingOnAzure:AzureKeyVault:ContainerName"]);
                            BlobClient blobClient = blobContainerClient.GetBlobClient(Configuration["HostingOnAzure:AzureKeyVault:KeysBlobName"]);

                            var certificateIdentifier = Configuration["HostingOnAzure:AzureKeyVault:CertificateIdentifier"];

                            dataProtectionBuilder.PersistKeysToAzureBlobStorage(blobClient);
                            // 1. Remove the call to ProtectKeysWithAzureKeyVault below for the first run to create the keys.xml blob in place.
                            // 2. Add the call to ProtectKeysWithAzureKeyVault for subsequent runs.so that keys.xml gets created - see the setup doc for more information
                            dataProtectionBuilder.ProtectKeysWithAzureKeyVault(new Uri(certificateIdentifier), tokenCredential);

                            // Azure Key Vault Access Policy must grant the following permissions to the blazor-boilerplate app: 
                            // - Secret Permissions: Get
                            // - Certificate Permissions: Get

                            // Retrieve the certificate and extract the secret so that we can build the new X509 certificate for later use by Identity Server
                            var certificateClient = new CertificateClient(vaultUri: new Uri(Configuration["HostingOnAzure:AzureKeyVault:KeyVaultUri"]), credential: new DefaultAzureCredential());
                            var secretClient = new SecretClient(vaultUri: new Uri(Configuration["HostingOnAzure:AzureKeyVault:KeyVaultUri"]), credential: new DefaultAzureCredential());
                            KeyVaultCertificateWithPolicy certificateWithPolicy = certificateClient.GetCertificateAsync(Configuration["HostingOnAzure:AzureKeyVault:CertificateName"]).GetAwaiter().GetResult(); // retrieves latest version of certificate
                            KeyVaultSecretIdentifier secretIdentifier = new(certificateWithPolicy.SecretId);
                            KeyVaultSecret secret = secretClient.GetSecretAsync(secretIdentifier.Name, secretIdentifier.Version).GetAwaiter().GetResult();
                            byte[] privateKeyBytes = Convert.FromBase64String(secret.Value);

                            cert = new X509Certificate2(privateKeyBytes, (string)null, X509KeyStorageFlags.MachineKeySet);
                        }
                    }
                    else // if app id and app secret are used
                        throw new NotImplementedException();
                }
                else
                    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysLocalFolder));

                //TODO this implementation does not consider certificate expiration 
                if (Convert.ToBoolean(Configuration[$"{projectName}:UseLocalCertStore"]) == true)
                {
                    var certificateThumbprint = Configuration[$"{projectName}:CertificateThumbprint"];

                    var storeLocation = StoreLocation.LocalMachine;

                    dynamic storeName = "WebHosting";

                    if (OperatingSystem.IsLinux())
                    {
                        storeLocation = StoreLocation.CurrentUser;
                        storeName = StoreName.My;
                    }

                    using X509Store store = new(storeName, storeLocation);
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);
                    if (certs.Count > 0)
                    {
                        cert = certs[0];
                    }
                    else
                    {
                        var certPath = Path.Combine(_environment.ContentRootPath, "AuthSample.pfx");

                        if (File.Exists(certPath))
                        {
                            string certificatePassword = Configuration[$"{projectName}:CertificatePassword"] ?? "Admin123";
                            cert = new X509Certificate2(certPath, certificatePassword,
                                                X509KeyStorageFlags.MachineKeySet |
                                                X509KeyStorageFlags.PersistKeySet |
                                                X509KeyStorageFlags.Exportable);
                        }
                    }

                    store.Close();
                }

                // pass the resulting certificate to Identity Server
                if (cert != null)
                {
                    identityServerBuilder.AddSigningCredential(cert);
                    Log.Logger.Information($"Added certificate {cert.Subject} to Identity Server");
                }
                else if (OperatingSystem.IsWindows())
                {
                    Log.Logger.Debug("Trying to use WebHosting Certificate for Identity Server");
                    identityServerBuilder.AddWebHostingCertificate();
                }
                else
                {
                    throw new Exception("Missing Certificate for Identity Server");
                }
            }

            var authBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authAuthority;
                options.RequireHttpsMetadata = _environment.IsProduction();
                options.Audience = IdentityServerConfig.LocalApiName;
                options.TokenValidationParameters.ValidTypes = new[] { JwtTypes.AccessToken };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments(Constants.HubPaths.Chat))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            #region ExternalAuthProviders
            //https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/samples/SocialSample/Startup.cs
            //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins
            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Google:Enabled"] ?? "false"))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["ExternalAuthProviders:Google:ClientId"];
                    options.ClientSecret = Configuration["ExternalAuthProviders:Google:ClientSecret"];

                    options.AuthorizationEndpoint += "?prompt=consent"; // Hack so we always get a refresh token, it only comes on the first authorization response
                    options.AccessType = "offline";
                    options.SaveTokens = true;
                    options.Events = new OAuthEvents()
                    {
                        OnRemoteFailure = HandleOnRemoteFailure
                    };
                    options.ClaimActions.MapJsonSubKey("urn:google:image", "image", "url");
                    options.ClaimActions.Remove(ClaimTypes.GivenName);
                });
            }

            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Facebook:Enabled"] ?? "false"))
            {
                // You must first create an app with Facebook and add its ID and Secret to your user-secrets.
                // https://developers.facebook.com/apps/
                // https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow#login
                authBuilder.AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.AppId = Configuration["ExternalAuthProviders:Facebook:AppId"];
                    options.AppSecret = Configuration["ExternalAuthProviders:Facebook:AppSecret"];

                    options.Scope.Add("email");
                    options.Fields.Add("name");
                    options.Fields.Add("email");
                    options.SaveTokens = true;
                    options.Events = new OAuthEvents()
                    {
                        OnRemoteFailure = HandleOnRemoteFailure
                    };
                });
            }

            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Twitter:Enabled"] ?? "false"))
            {
                // You must first create an app with Twitter and add its key and Secret to your user-secrets.
                // https://apps.twitter.com/
                // https://developer.twitter.com/en/docs/basics/authentication/api-reference/access_token
                authBuilder.AddTwitter(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ConsumerKey = Configuration["ExternalAuthProviders:Twitter:ConsumerKey"];
                    options.ConsumerSecret = Configuration["ExternalAuthProviders:Twitter:ConsumerSecret"];

                    // http://stackoverflow.com/questions/22627083/can-we-get-email-id-from-twitter-oauth-api/32852370#32852370
                    // http://stackoverflow.com/questions/36330675/get-users-email-from-twitter-api-for-external-login-authentication-asp-net-mvc?lq=1
                    options.RetrieveUserDetails = true;
                    options.SaveTokens = true;
                    options.ClaimActions.MapJsonKey("urn:twitter:profilepicture", "profile_image_url", ClaimTypes.Uri);
                    options.Events = new TwitterEvents()
                    {
                        OnRemoteFailure = HandleOnRemoteFailure
                    };
                });
            }

            //https://github.com/xamarin/Essentials/blob/master/Samples/Sample.Server.WebAuthenticator/Startup.cs
            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Apple:Enabled"] ?? "false"))
            {
                authBuilder.AddApple(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["ExternalAuthProviders:Apple:ClientId"];
                    options.KeyId = Configuration["ExternalAuthProviders:Apple:KeyId"];
                    options.TeamId = Configuration["ExternalAuthProviders:Apple:TeamId"];

                    options.UsePrivateKey(keyId
                       => _environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
                    options.SaveTokens = true;
                });
            }

            // You must first create an app with Microsoft Account and add its ID and Secret to your user-secrets.
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app
            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Microsoft:Enabled"] ?? "false"))
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["ExternalAuthProviders:Microsoft:ClientId"];
                    options.ClientSecret = Configuration["ExternalAuthProviders:Microsoft:ClientSecret"];

                    options.SaveTokens = true;
                    options.Scope.Add("offline_access");
                    options.Events = new OAuthEvents()
                    {
                        OnRemoteFailure = HandleOnRemoteFailure
                    };
                });
            }
            #endregion

            #region Authorization
            //Add Policies / Claims / Authorization - https://identityserver4.readthedocs.io/en/latest/topics/add_apis.html#advanced 
            services.AddScoped<EntityPermissions>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
            services.AddTransient<IAuthorizationHandler, EmailVerifiedHandler>();
            services.AddTransient<IAuthorizationHandler, PermissionRequirementHandler>();
            #endregion

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = RequireDigit;
                options.Password.RequiredLength = RequiredLength;
                options.Password.RequireNonAlphanumeric = RequireNonAlphanumeric;
                options.Password.RequireUppercase = RequireUppercase;
                options.Password.RequireLowercase = RequireLowercase;
                //options.Password.RequiredUniqueChars = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                if (Convert.ToBoolean(Configuration[$"{projectName}:RequireConfirmedEmail"] ?? "false"))
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                }
            });

            #region Cookies
            // cookie policy to deal with temporary browser incompatibilities
            services.AddSameSiteCookiePolicy();

            //https://docs.microsoft.com/en-us/aspnet/core/security/gdpr
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => false; //consent not required
                // requires using Microsoft.AspNetCore.Http;
                //options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.ConfigureExternalCookie(options =>
            // {
            // macOS login fix
            //options.Cookie.SameSite = SameSiteMode.None;
            //});

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromDays(Convert.ToDouble(Configuration[$"{projectName}:CookieExpireTimeSpanDays"] ?? "30"));
                options.LoginPath = Constants.Settings.LoginPath;
                //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                // ReturnUrlParameter requires
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;

                // Suppress redirect on API URLs in ASP.NET Core -> https://stackoverflow.com/a/56384729/54159
                options.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = Status403Forbidden;
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            });
            #endregion

            services.AddMvc().AddNewtonsoftJson(opt =>
            {
                // Set Breeze defaults for entity serialization
                var ss = JsonSerializationFns.UpdateWithDefaults(opt.SerializerSettings);
                if (ss.ContractResolver is DefaultContractResolver resolver)
                {
                    resolver.NamingStrategy = null;  // remove json camelCasing; names are converted on the client.
                }
                if (_environment.IsDevelopment())
                {
                    ss.Formatting = Newtonsoft.Json.Formatting.Indented; // format JSON for debugging
                }
            })   // Add Breeze exception filter to send errors back to the client
            .AddMvcOptions(o => { o.Filters.Add(new GlobalExceptionFilter()); })
            .AddViewLocalization().AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    return factory.Create(typeof(Global));
                };
            });  
            //.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<LocalizationRecordValidator>());

            services.AddFluentValidationAutoValidation();

            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                o.DetailedErrors = Convert.ToBoolean(Configuration[$"{projectName}:DetailedErrors"] ?? bool.FalseString);

                if (_environment.IsDevelopment())
                {
                    o.DetailedErrors = true;
                }
            }).AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 131072;
            });

            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, UserIdProvider>();

            if (_enableAPIDoc)
                services.AddOpenApiDocument(document =>
                {
                    document.Title = "BlazorBoilerplate API";
                    document.Version = typeof(Startup).GetTypeInfo().Assembly.GetName().Version.ToString();
                    document.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.OAuth2,
                        Description = "Local Identity Server",
                        OpenIdConnectUrl = $"{authAuthority}/.well-known/openid-configuration", //not working
                        Flow = OpenApiOAuth2Flow.AccessCode,
                        Flows = new OpenApiOAuthFlows()
                        {
                            AuthorizationCode = new OpenApiOAuthFlow()
                            {
                                Scopes = new Dictionary<string, string>
                                {
                                { LocalApi.ScopeName, IdentityServerConfig.LocalApiName }
                                },
                                AuthorizationUrl = $"{authAuthority}/connect/authorize",
                                TokenUrl = $"{authAuthority}/connect/token"
                            },
                        }
                    }); ;

                    document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
                    //      new OperationSecurityScopeProcessor("bearer"));
                });

            services.AddScoped<IUserSession, UserSession>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Add(ServiceDescriptor.Scoped(typeof(ITenantSettings<>), typeof(TenantSettingsManager<>)));

            services.AddTransient<IEmailFactory, EmailFactory>();

            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<IAdminManager, AdminManager>();
            services.AddTransient<IEmailManager, EmailManager>();
            services.AddTransient<IExternalAuthManager, ExternalAuthManager>();

            #region Automapper
            //Automapper to map DTO to Models https://www.c-sharpcorner.com/UploadFile/1492b1/crud-operations-using-automapper-in-mvc-application/
            var automapperConfig = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile(new MappingProfile());
            });

            var autoMapper = automapperConfig.CreateMapper();

            services.AddSingleton(autoMapper);
            #endregion

            /* ServerSideBlazor */
            services.AddScoped<IAccountApiClient, AccountApiClient>();
            services.AddScoped<AppState>();

            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            // if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            // {
            services.AddScoped(s =>
            {
                // creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                var navigationManager = s.GetRequiredService<NavigationManager>();
                var httpContextAccessor = s.GetRequiredService<IHttpContextAccessor>();
                var cookies = httpContextAccessor.HttpContext.Request.Cookies;
                var httpClientHandler = new HttpClientHandler() { UseCookies = false };
                if (_environment.IsDevelopment())
                {
                    // Return 'true' to allow certificates that are untrusted/invalid
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                var client = new HttpClient(httpClientHandler);
                if (cookies.Any())
                {
                    var cks = new List<string>();

                    foreach (var cookie in cookies)
                    {
                        cks.Add($"{cookie.Key}={cookie.Value}");
                    }

                    client.DefaultRequestHeaders.Add("Cookie", string.Join(';', cks));
                }

                client.BaseAddress = new Uri(navigationManager.BaseUri);

                return client;
            });
            // }

            services.AddScoped<ILocalizationApiClient, LocalizationApiClient>();
            services.AddScoped<IApiClient, ApiClient>();

            // Authentication providers
            Log.Logger.Debug("Removing AuthenticationStateProvider...");
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(AuthenticationStateProvider));
            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }

            Log.Logger.Debug("Adding AuthenticationStateProvider...");
            services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
            /**********************/

            services.AddModules();

            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                Log.Logger.Debug($"Total Services Registered: {services.Count}");
                foreach (var service in services)
                    Log.Logger.Debug($"\n\tService: {service.ServiceType.FullName}\n\tLifetime: {service.Lifetime}\n\tInstance: {service.ImplementationType?.FullName}");
            }
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRequestLocalization();

            // cookie policy to deal with temporary browser incompatibilities
            app.UseCookiePolicy();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging(); //ClientSideBlazor
            }
            else
            {
                app.UseHttpsRedirection();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //    app.UseHsts(); //HSTS Middleware (UseHsts) to send HTTP Strict Transport Security Protocol (HSTS) headers to clients.
            }

            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles(); //ClientSideBlazor

            app.UseRouting();

            app.UseIdentityServer();
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseMultiTenant();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var databaseInitializer = serviceScope.ServiceProvider.GetService<IDatabaseInitializer>();
                databaseInitializer.SeedAsync().Wait();

                var localizationProvider = serviceScope.ServiceProvider.GetService<ILocalizationProvider>();

                var localizationDbContext = serviceScope.ServiceProvider.GetService<LocalizationDbContext>();

                localizationProvider.Init(localizationDbContext.LocalizationRecords.Include(i => i.PluralTranslations), localizationDbContext.PluralFormRules);
            }

            app.UseMiddleware<UserSessionMiddleware>();

            // before app.UseMultiTenant() make injected TenantInfo == null
            app.UseMiddleware<APIResponseRequestLoggingMiddleware>();

            if (_enableAPIDoc)
            {
                app.UseOpenApi();
                app.UseSwaggerUi3(settings =>
                {
                    settings.OAuth2Client = new OAuth2ClientSettings()
                    {
                        AppName = projectName,
                        ClientId = IdentityServerConfig.SwaggerClientID,
                        UsePkceWithAuthorizationCodeGrant = true
                    };
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Index");

                // new SignalR endpoint routing setup
                endpoints.MapHub<Hubs.ChatHub>(Constants.HubPaths.Chat);
            });

            Program.Sync.Release();
        }

#pragma warning disable CS1998 
        private async Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            var msg = context.Failure.Message.Split(Environment.NewLine).Select(s => s + Environment.NewLine).Aggregate((s1, s2) => s1 + s2);

            if (context.Properties != null)
                foreach (var pair in context.Properties.Items)
                    msg = $"{msg}{Environment.NewLine}-{pair.Key}={pair.Value}";

            Log.Logger.Error($"External authentication error: {msg}");

            context.Response.Redirect($"/externalauth/error/{ErrorEnum.ExternalAuthError}");

            context.HandleResponse();
        }
    }
}