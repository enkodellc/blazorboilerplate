using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using AutoMapper;

#if ServerSideBlazor

using BlazorBoilerplate.CommonUI;
using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.CommonUI.Services.Implementations;
using BlazorBoilerplate.CommonUI.States;

using MatBlazor;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using System.Net.Http;

#endif

using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Data.Mapping;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Middleware;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;

using IdentityServer4;
using IdentityServer4.AccessTokenValidation;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BlazorBoilerplate.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var useSqlServer = Convert.ToBoolean(Configuration["BlazorBoilerplate:UseSqlServer"] ?? "false");
            var dbConnString = useSqlServer
                ? Configuration.GetConnectionString("DefaultConnection")
                : $"Filename={Configuration.GetConnectionString("SqlLiteConnectionFileName")}";

            var authAuthority = Configuration["BlazorBoilerplate:IS4ApplicationUrl"].TrimEnd('/');

            void DbContextOptionsBuilder(DbContextOptionsBuilder builder)
            {
                if (useSqlServer)
                {
                    builder.UseSqlServer(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
                }
                else if (Convert.ToBoolean(Configuration["BlazorBoilerplate:UsePostgresServer"] ?? "false"))
                {
                    builder.UseNpgsql(Configuration.GetConnectionString("PostgresConnection"), sql => sql.MigrationsAssembly(migrationsAssembly));
                }
                else
                {
                    builder.UseSqlite(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
                }
            }

            services.AddDbContext<ApplicationDbContext>(DbContextOptionsBuilder);

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                AdditionalUserClaimsPrincipalFactory>();

            // Adds IdentityServer
            var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = authAuthority;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
              .AddConfigurationStore(options =>
              {
                  options.ConfigureDbContext = DbContextOptionsBuilder;
              })
              .AddOperationalStore(options =>
              {
                  options.ConfigureDbContext = DbContextOptionsBuilder;

                  // this enables automatic token cleanup. this is optional.
                  options.EnableTokenCleanup = true;
                  options.TokenCleanupInterval = 3600; //In Seconds 1 hour
              })
              .AddAspNetIdentity<ApplicationUser>();

            X509Certificate2 cert = null;

            if (_environment.IsDevelopment())
            {
                // The AddDeveloperSigningCredential extension creates temporary key material for signing tokens.
                // This might be useful to get started, but needs to be replaced by some persistent key material for production scenarios.
                // See http://docs.identityserver.io/en/release/topics/crypto.html#refcrypto for more information.
                // https://stackoverflow.com/questions/42351274/identityserver4-hosting-in-iis
                //.AddDeveloperSigningCredential(true, @"C:\tempkey.rsa")
                identityServerBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                // Works for IIS, finds cert by the thumbprint in appsettings.json
                // Make sure Certificate is in the Web Hosting folder && installed to LocalMachine or update settings below
                var useLocalCertStore = Convert.ToBoolean(Configuration["BlazorBoilerplate:UseLocalCertStore"]);
                var certificateThumbprint = Configuration["BlazorBoilerplate:CertificateThumbprint"];

                if (useLocalCertStore)
                {
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
                else
                {
                    // Azure deployment, will be used if deployed to Azure - Not tested
                    //var vaultConfigSection = Configuration.GetSection("Vault");
                    //var keyVaultService = new KeyVaultCertificateService(vaultConfigSection["Url"], vaultConfigSection["ClientId"], vaultConfigSection["ClientSecret"]);
                    ////cert = keyVaultService.GetCertificateFromKeyVault(vaultConfigSection["CertificateName"]);

                    /// I was informed that this will work as a temp solution in Azure
                    cert = new X509Certificate2("AuthSample.pfx", "Admin123",
                        X509KeyStorageFlags.MachineKeySet |
                        X509KeyStorageFlags.PersistKeySet |
                        X509KeyStorageFlags.Exportable);
                }
                identityServerBuilder.AddSigningCredential(cert);
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
                options.ApiName = IdentityServerConfig.ApiName;
            });

            //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-3.1
            if (Convert.ToBoolean(Configuration["ExternalAuthProviders:Google:Enabled"] ?? "false"))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["ExternalAuthProviders:Google:ClientId"];
                    options.ClientSecret = Configuration["ExternalAuthProviders:Google:ClientSecret"];
                });
            }
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

            //Add Policies / Claims / Authorization - https://stormpath.com/blog/tutorial-policy-based-authorization-asp-net-core
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
                options.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
                options.AddPolicy(Policies.IsReadOnly, Policies.IsReadOnlyPolicy());
                options.AddPolicy(Policies.IsMyDomain, Policies.IsMyDomainPolicy());  // valid only on serverside operations
            });

            services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();

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
                if (Convert.ToBoolean(Configuration["BlazorBoilerplate:RequireConfirmedEmail"] ?? "false"))
                {
                    options.User.RequireUniqueEmail = false;
                    options.SignIn.RequireConfirmedEmail = true;
                }
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.ConfigureExternalCookie(options =>
            {
                // macOS login fix
                options.Cookie.SameSite = SameSiteMode.None;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // macOS login fix
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.HttpOnly = false;

                // Suppress redirect on API URLs in ASP.NET Core -> https://stackoverflow.com/a/56384729/54159
                options.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int) (HttpStatusCode.Unauthorized);
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
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
                    document.Info.Version     = "0.6.0";
                    document.Info.Title       = "Blazor Boilerplate";
#if ServerSideBlazor
                    document.Info.Description = "Blazor Boilerplate / Starter Template using the  Server Side Version";
#endif
#if ClientSideBlazor
                    document.Info.Description = "Blazor Boilerplate / Starter Template using the Client Side / Webassembly Version.";
#endif
                };
            });

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddScoped<IUserSession, UserSession>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IApiLogService, ApiLogService>();
            services.AddTransient<ITodoService, ToDoService>();
            services.AddTransient<IMessageService, MessageService>();

            // DB Creation and Seeding
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            //Automapper to map DTO to Models https://www.c-sharpcorner.com/UploadFile/1492b1/crud-operations-using-automapper-in-mvc-application/
            var automapperConfig = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile(new MappingProfile());
            });

            var autoMapper = automapperConfig.CreateMapper();

            services.AddSingleton(autoMapper);

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
            services.AddServerSideBlazor();

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

            Log.Logger.Debug($"Total Services Registered: {services.Count}");
            foreach (var service in services)
            {
                Log.Logger.Debug($"\n      Service: {service.ServiceType.FullName}\n      Lifetime: {service.Lifetime}\n      Instance: {service.ImplementationType?.FullName}");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            EmailTemplates.Initialize(env);

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var databaseInitializer = serviceScope.ServiceProvider.GetService<IDatabaseInitializer>();
                databaseInitializer.SeedAsync().Wait();
            }

            app.UseResponseCompression(); // This must be before the other Middleware if that manipulates Response

            // A REST API global exception handler and response wrapper for a consistent API
            // Configure API Loggin in appsettings.json - Logs most API calls. Great for debugging and user activity audits
            app.UseMiddleware<APIResponseRequestLoggingMiddleware>(Convert.ToBoolean(Configuration["BlazorBoilerplate:EnableAPILogging:Enabled"] ?? "true"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if ClientSideBlazor
                app.UseBlazorDebugging();
#endif
            }
            else
            {
              // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
              //    app.UseHsts(); //HSTS Middleware (UseHsts) to send HTTP Strict Transport Security Protocol (HSTS) headers to clients.
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

#if ClientSideBlazor
            app.UseClientSideBlazorFiles<Client.Startup>();
#endif

            app.UseRouting();
            //app.UseAuthentication(); //Removed for IS4
            app.UseIdentityServer();
            app.UseAuthorization();

            //Must be AFTER the Auth middleware to get the User/Identity info
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

#if ClientSideBlazor
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index_csb.html");
#else
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/index_ssb");
#endif
            });

        }
    }
}
