using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage.Stores;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Storage
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string projectName = nameof(BlazorBoilerplate);

        public static IServiceCollection RegisterStorage(this IServiceCollection services, IConfiguration configuration)
        {
            #region Multitenancy

            services.AddDbContext<TenantStoreDbContext>(builder => GetDbContextOptions<TenantStoreDbContext>(builder, configuration));

            services.AddMultiTenant()
                .WithDelegateStrategy(context =>
                {
                    if (((HttpContext)context).User.IsAuthenticated())
                    {
                        Claim c = ((HttpContext)context).User.Claims.SingleOrDefault(c => c.Type == "TenantId");
                        if (c != null)
                        {
                            string tenantId = c.Value;
                            return Task.FromResult(tenantId);
                        }
                    }
                    return Task.FromResult(projectName);
                })
                .WithEFCoreStore<TenantStoreDbContext>()
                .WithFallbackStrategy(configuration[$"{projectName}:DefaultTenantId"] ?? projectName);

            services.AddTransient<ITenantStore, TenantStore>();

            #endregion Multitenancy

            services.AddDbContext<ApplicationDbContext>(builder => GetDbContextOptions<ApplicationDbContext>(builder, configuration)); // Look into the way we initialise the PB ways. Look at the old way they did this, with side effects on the builder.
            services.AddScoped(s => s.GetRequiredService<ApplicationDbContext>() as IApplicationDbContext);

            services.AddTransient<IMessageStore, MessageStore>();
            services.AddTransient<IUserProfileStore, UserProfileStore>();
            services.AddTransient<IToDoStore, ToDoStore>();
            services.AddTransient<IApiLogStore, ApiLogStore>();

            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            return services;
        }

        public static void GetDbContextOptions<T>(DbContextOptionsBuilder builder, IConfiguration configuration) where T : DbContext
        {
            var migrationsAssembly = typeof(T).GetTypeInfo().Assembly.GetName().Name;
            var useSqlServer = Convert.ToBoolean(configuration[$"{projectName}:UseSqlServer"] ?? "false");
            var dbConnString = useSqlServer
                ? configuration.GetConnectionString("DefaultConnection")
                : $"Filename={configuration.GetConnectionString("SqlLiteConnectionFileName")}";

            if (useSqlServer)
            {
                builder.UseSqlServer(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
            else if (Convert.ToBoolean(configuration[$"{projectName}:UsePostgresServer"] ?? "false"))
            {
                builder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"), sql => sql.MigrationsAssembly(migrationsAssembly));
            }
            else
            {
                builder.UseSqlite(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
        }

        public static IIdentityServerBuilder AddIdentityServerStores(this IIdentityServerBuilder builder, IConfiguration configuration)
        => builder.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = x => GetDbContextOptions<ApplicationDbContext>(x, configuration);
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = x => GetDbContextOptions<ApplicationDbContext>(x, configuration);

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;

                options.TokenCleanupInterval = 3600; //In Seconds 1 hour
            });
    }
}