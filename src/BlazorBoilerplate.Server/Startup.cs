using AutoMapper;
using BlazorBoilerplate.Server.Authorization;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Data.Mapping;
using BlazorBoilerplate.Server.Helpers;
using BlazorBoilerplate.Server.Middleware;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => {
                if (Convert.ToBoolean(Configuration["BlazorBoilerplate:UseSqlServer"] ?? "false"))
                {
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")); //SQL Server Database
                }
                else
                {
                    options.UseSqlite($"Filename={Configuration.GetConnectionString("SqlLiteConnectionFileName")}");  // Sql Lite / file database
                }
            });

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                AdditionalUserClaimsPrincipalFactory>();

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

            services.ConfigureApplicationCookie(options =>
            {
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
                    document.Info.Version     = "v0.2.2";
                    document.Info.Title       = "Blazor Boilerplate";
                    document.Info.Description = "Blazor Boilerplate / Starter Template using the  (ASP.NET Core Hosted) (dotnet new blazorhosted) model. Hosted by an ASP.NET Core server";
                };
            });

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserSession, UserSession>();
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IApiLogService, ApiLogService>();
            services.AddTransient<ITodoService, ToDoService>();
            services.AddTransient<IMessageService, MessageService>();
            services.AddTransient<IApplicationDbContextSeed, ApplicationDbContextSeed>();

            // AutoMapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            //Automapper to map DTO to Models https://www.c-sharpcorner.com/UploadFile/1492b1/crud-operations-using-automapper-in-mvc-application/

            var automapperConfig = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile(new MappingProfile());
            });

            var autoMapper = automapperConfig.CreateMapper();

            services.AddSingleton(autoMapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationDbContextSeed applicationDbContextSeed)
        {
            EmailTemplates.Initialize(env);
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            }

            app.UseResponseCompression(); // This must be before the other Middleware if that manipulates Response

            app.UseMiddleware<UserSessionMiddleware>();
            // A REST API global exception handler and response wrapper for a consistent API
            // Configure API Loggin in appsettings.json - Logs most API calls. Great for debugging and user activity audits
            app.UseMiddleware<APIResponseRequestLogginMiddleware>(Convert.ToBoolean(Configuration["BlazorBoilerplate:EnableAPILogging:Enabled"] ?? "true"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }
            //else
            //{
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts(); //HSTS Middleware (UseHsts) to send HTTP Strict Transport Security Protocol (HSTS) headers to clients.
            //}

            //app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Startup>();

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // NSwag
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                // new SignalR endpoint routing setup
                endpoints.MapHub<Hubs.ChatHub>("/chathub");
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });

            //Seed Database
            applicationDbContextSeed.SeedDb();
        }
    }
}
