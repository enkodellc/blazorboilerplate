using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using AutoMapper;

//-:cnd:noEmit
#if ServerSideBlazor

using BlazorBoilerplate.CommonUI;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Services;

using MatBlazor;

using Microsoft.AspNetCore.Components.Authorization;

using System.Net.Http;

#endif
//-:cnd:noEmit

using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Storage.Mapping;
using IdentityServer4;
using IdentityServer4.AccessTokenValidation;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using System.Reflection;
using Microsoft.AspNetCore.Localization;
using BlazorBoilerplate.Localization;
using Microsoft.AspNetCore.DataProtection;
using BlazorBoilerplate.Shared.Providers;

namespace BlazorBoilerplate.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _environment;

        private readonly string projectName = nameof(BlazorBoilerplate);

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization()
                .Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(Settings.SupportedCultures[0]);
                options.AddSupportedCultures(Settings.SupportedCultures);
                options.AddSupportedUICultures(Settings.SupportedCultures);
            });

            var dataProtectionBuilder = services.AddDataProtection().SetApplicationName(nameof(BlazorBoilerplate));

            var authAuthority = Configuration[$"{projectName}:IS4ApplicationUrl"].TrimEnd('/');

            services.RegisterStorage(Configuration);

            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                AdditionalUserClaimsPrincipalFactory>();

            // cookie policy to deal with temporary browser incompatibilities
            services.AddSameSiteCookiePolicy();

            // Adds IdentityServer https://identityserver4.readthedocs.io/en/latest/reference/options.html
            var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = authAuthority;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
              .AddIdentityServerStores(Configuration)
              .AddAspNetIdentity<ApplicationUser>(); //https://identityserver4.readthedocs.io/en/latest/reference/aspnet_identity.html

            X509Certificate2 cert = null;

            var keysFolder = Path.Combine(_environment.ContentRootPath, "Keys");

            if (_environment.IsDevelopment())
            {
                // The AddDeveloperSigningCredential extension creates temporary key material for signing tokens.
                // This might be useful to get started, but needs to be replaced by some persistent key material for production scenarios.
                // See http://docs.identityserver.io/en/release/topics/crypto.html#refcrypto for more information.
                // https://stackoverflow.com/questions/42351274/identityserver4-hosting-in-iis
                //.AddDeveloperSigningCredential(true, @"C:\tempkey.rsa")
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
                                var certificate = System.Convert.FromBase64String(certificateBundle.Value);
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

                // using local cert store
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
                            // import PFX
                            cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "AuthSample.pfx"), "Admin123",
                                                X509KeyStorageFlags.MachineKeySet |
                                                X509KeyStorageFlags.PersistKeySet |
                                                X509KeyStorageFlags.Exportable);
                            // save certificate and private key
                            X509Store storeMy = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
                            storeMy.Open(OpenFlags.ReadWrite);
                            storeMy.Add(cert);
                        }
                        store.Close();
                    }
                }

                // pass the resulting certificate to Identity Server
                if (cert != null)
                {
                    identityServerBuilder.AddSigningCredential(cert);
                }
                else
                {
                    throw new FileNotFoundException("No certificate for Identity Server could be retrieved.");
                }
            }

            services.AddProtectedBrowserStorage();

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
            })
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = authAuthority;
                options.SupportedTokens = SupportedTokens.Jwt;
                options.RequireHttpsMetadata = _environment.IsProduction() ? true : false;
                options.ApiName = IdentityServerConfig.ApiName;
            });

            //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins
            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Google:Enabled"] ?? "false"))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["ExternalAuthProviders:Google:ClientId"];
                    options.ClientSecret = Configuration["ExternalAuthProviders:Google:ClientSecret"];
                });
            }

            //Add Policies / Claims / Authorization - https://identityserver4.readthedocs.io/en/latest/topics/add_apis.html#advanced
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
                options.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
                options.AddPolicy(Policies.IsReadOnly, Policies.IsReadOnlyPolicy());
                options.AddPolicy(Policies.IsMyDomain, Policies.IsMyDomainPolicy());  // valid only on serverside operations
            });

            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
            services.AddTransient<IAuthorizationHandler, PermissionRequirementHandler>();

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
                    options.User.RequireUniqueEmail = false;
                    options.SignIn.RequireConfirmedEmail = true;
                }
            });

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
                options.Cookie.HttpOnly = false; //TODO should be true for security
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Account/Login";
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

            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR();

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = typeof(Startup).GetTypeInfo().Assembly.GetName().Version.ToString();
                    document.Info.Title = "BlazorBoilerplate";
                    //-:cnd:noEmit
#if ServerSideBlazor
                    document.Info.Description = "Blazor Boilerplate / Starter Template using the  Server Side Version";
#endif
                    //-:cnd:noEmit
                    //-:cnd:noEmit
#if ClientSideBlazor
                    document.Info.Description = "Blazor Boilerplate / Starter Template using the Client Side / Webassembly Version.";
#endif
                    //-:cnd:noEmit
                };
            });

            services.AddScoped<IUserSession, UserSession>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<IAdminManager, AdminManager>();
            services.AddTransient<IApiLogManager, ApiLogManager>();
            services.AddTransient<IDbLogManager, DbLogManager>();
            services.AddTransient<IEmailManager, EmailManager>();
            services.AddTransient<IExternalAuthManager, ExternalAuthManager>(); // Currently not being used.
            services.AddTransient<IMessageManager, MessageManager>();
            services.AddTransient<ITodoManager, ToDoManager>();
            services.AddTransient<IUserProfileManager, UserProfileManager>();
            services.AddTransient<ITenantManager, TenantManager>();

            //Automapper to map DTO to Models https://www.c-sharpcorner.com/UploadFile/1492b1/crud-operations-using-automapper-in-mvc-application/
            var automapperConfig = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile(new MappingProfile());
            });

            var autoMapper = automapperConfig.CreateMapper();

            services.AddSingleton(autoMapper);
            //-:cnd:noEmit
#if ServerSideBlazor

            services.AddScoped<IAuthorizeApi, AuthorizeApi>();
            services.AddScoped<IUserProfileApi, UserProfileApi>();
            services.AddScoped<AppState>();
            services.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomRight;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.ShowCloseButton = true;
                config.MaximumOpacity = 95;
                config.VisibleStateDuration = 3000;
            });

            // Setup HttpClient for server side
            services.AddScoped<HttpClient>();

            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                if (_environment.IsDevelopment())
                {
                    o.DetailedErrors = true;
                }
            });

            // Authentication providers

            Log.Logger.Debug("Removing AuthenticationStateProvider...");
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(AuthenticationStateProvider));
            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }

            Log.Logger.Debug("Adding AuthenticationStateProvider...");
            services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();

#endif
            //-:cnd:noEmit

            Log.Logger.Debug($"Total Services Registered: {services.Count}");
            foreach (var service in services)
            {
                Log.Logger.Debug($"\n      Service: {service.ServiceType.FullName}\n      Lifetime: {service.Lifetime}\n      Instance: {service.ImplementationType?.FullName}");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRequestLocalization();

            // cookie policy to deal with temporary browser incompatibilities
            app.UseCookiePolicy();

            EmailTemplates.Initialize(env);

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var databaseInitializer = serviceScope.ServiceProvider.GetService<IDatabaseInitializer>();
                databaseInitializer.SeedAsync().Wait();
            }

            // A REST API global exception handler and response wrapper for a consistent API
            // Configure API Loggin in appsettings.json - Logs most API calls. Great for debugging and user activity audits
            app.UseMiddleware<APIResponseRequestLoggingMiddleware>(Convert.ToBoolean(Configuration["BlazorBoilerplate:EnableAPILogging:Enabled"] ?? "true"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //-:cnd:noEmit
#if ClientSideBlazor
                app.UseWebAssemblyDebugging();
#endif
                //-:cnd:noEmit
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //    app.UseHsts(); //HSTS Middleware (UseHsts) to send HTTP Strict Transport Security Protocol (HSTS) headers to clients.
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //-:cnd:noEmit
#if ClientSideBlazor
            app.UseBlazorFrameworkFiles();
#endif
            //-:cnd:noEmit

            app.UseRouting();
            //app.UseAuthentication(); //Removed for IS4
            app.UseIdentityServer();
            app.UseAuthorization();

            //Must be AFTER the Auth middleware to get the User/Identity info
            app.UseMultiTenant();
            app.UseMiddleware<UserSessionMiddleware>();

            // NSwag
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                // new SignalR endpoint routing setup
                endpoints.MapHub<Hubs.ChatHub>("/chathub");
                //-:cnd:noEmit
#if ClientSideBlazor
                endpoints.MapFallbackToFile("index_csb.html");
#else
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/index_ssb");
#endif
                //-:cnd:noEmit
            });
        }
    }
}