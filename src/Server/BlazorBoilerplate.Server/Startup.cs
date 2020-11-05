using AutoMapper;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Server.Factories;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
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
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization; //ServerSideBlazor
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http; //ServerSideBlazor
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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

            var keysFolder = Path.Combine(_environment.ContentRootPath, "Keys");

            if (_environment.IsDevelopment())
            {
                // The AddDeveloperSigningCredential extension creates temporary key material tempkey.jwk for signing tokens.
                // This might be useful to get started, but needs to be replaced by some persistent key material for production scenarios.
                // See http://docs.identityserver.io/en/release/topics/crypto.html#refcrypto for more information.
                // https://stackoverflow.com/questions/42351274/identityserver4-hosting-in-iis

                identityServerBuilder.AddDeveloperSigningCredential();

                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
            }
            else
            {
                // running on azure
                // please make sure to replace your vault URI and your certificate name in appsettings.json!
                if (Convert.ToBoolean(Configuration["HostingOnAzure:RunsOnAzure"]) == true)
                {
                    // if we use a key vault
                    if (Convert.ToBoolean(Configuration["HostingOnAzure:AzurekeyVault:UsingKeyVault"]) == true)
                    {
                        //https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview
                        dataProtectionBuilder.PersistKeysToAzureBlobStorage(new Uri("<blobUriWithSasToken>"))
                            .ProtectKeysWithAzureKeyVault("<keyIdentifier>", "<clientId>", "<clientSecret>");

                        // if managed app identity is used
                        if (Convert.ToBoolean(Configuration["HostingOnAzure:AzurekeyVault:UseManagedAppIdentity"]) == true)
                        {
                            try
                            {
                                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

                                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                                var certificateBundle = keyVaultClient.GetSecretAsync(Configuration["HostingOnAzure:AzureKeyVault:VaultURI"], Configuration["HostingOnAzure:AzurekeyVault:CertificateName"]).GetAwaiter().GetResult();
                                var certificate = Convert.FromBase64String(certificateBundle.Value);
                                cert = new X509Certificate2(certificate, (string)null, X509KeyStorageFlags.MachineKeySet);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    else // if app id and app secret are used
                        throw new NotImplementedException();
                }
                else
                    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

                //TODO this implementation does not consider certificate expiration 
                if (Convert.ToBoolean(Configuration[$"{projectName}:UseLocalCertStore"]) == true)
                {
                    var certificateThumbprint = Configuration[$"{projectName}:CertificateThumbprint"];
                    using (X509Store store = new X509Store("WebHosting", StoreLocation.LocalMachine))
                    {
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
                                cert = new X509Certificate2(certPath, "Admin123",
                                                    X509KeyStorageFlags.MachineKeySet |
                                                    X509KeyStorageFlags.PersistKeySet |
                                                    X509KeyStorageFlags.Exportable);
                        }

                        store.Close();
                    }
                }

                // pass the resulting certificate to Identity Server
                if (cert != null)
                {
                    identityServerBuilder.AddSigningCredential(cert);
                    Log.Logger.Information($"Added certificate {cert.Subject} to Identity Server");
                }
                else
                {
                    Log.Logger.Debug("Trying to use WebHosting Certificate for Identity Server");
                    identityServerBuilder.AddWebHostingCertificate();
                }
            }

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
            })
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = authAuthority;
                options.SupportedTokens = SupportedTokens.Jwt;
                options.RequireHttpsMetadata = _environment.IsProduction() ? true : false;
                options.ApiName = IdentityServerConfig.LocalApiName;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/chathub")))
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
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
                options.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
                options.AddPolicy(Policies.IsReadOnly, Policies.IsReadOnlyPolicy());
                options.AddPolicy(Policies.IsMyDomain, Policies.IsMyDomainPolicy());  // valid only on serverside operations

                options.AddPolicy(Policies.TwoFactorEnabled, Policies.IsTwoFactorEnabledPolicy());
            });

            services.AddScoped<ApplicationPermissions>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
            services.AddTransient<IAuthorizationHandler, PermissionRequirementHandler>();
            #endregion

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                //options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // Require Confirmed Email User settings
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
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
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
            }).AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<LocalizationRecordValidator>());

            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                if (_environment.IsDevelopment())
                {
                    o.DetailedErrors = true;
                }
            });

            services.AddSignalR();

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
            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                services.AddScoped(s =>
                {
                    // creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                    var navigationManager = s.GetRequiredService<NavigationManager>();
                    var httpContextAccessor = s.GetRequiredService<IHttpContextAccessor>();
                    var cookies = httpContextAccessor.HttpContext.Request.Cookies;
                    var client = new HttpClient(new HttpClientHandler { UseCookies = false });
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
            }

            services.AddTransient<ILocalizationApiClient, LocalizationApiClient>();
            services.AddTransient<IApiClient, ApiClient>();

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //    app.UseHsts(); //HSTS Middleware (UseHsts) to send HTTP Strict Transport Security Protocol (HSTS) headers to clients.
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles(); //ClientSideBlazor

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
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
                endpoints.MapHub<Hubs.ChatHub>("/chathub");
            });
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