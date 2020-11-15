using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Shared;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace BlazorBoilerplate.Storage
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string projectName = nameof(BlazorBoilerplate);

        public static IServiceCollection RegisterStorage(this IServiceCollection services, IConfiguration configuration)
        {
            #region Multitenancy

            services.AddDbContext<TenantStoreDbContext>(builder => GetDbContextOptions<TenantStoreDbContext>(builder, configuration));

            services.AddMultiTenant<TenantInfo>()
                .WithHostStrategy("__tenant__")
                .WithEFCoreStore<TenantStoreDbContext, TenantInfo>()
                .WithStaticStrategy(Settings.DefaultTenantId);

            #endregion Multitenancy

            services.AddDbContext<ApplicationDbContext>(builder => GetDbContextOptions<ApplicationDbContext>(builder, configuration));
            services.AddScoped<ApplicationPersistenceManager>();

            services.AddDbContext<LocalizationDbContext>(builder => GetDbContextOptions<LocalizationDbContext>(builder, configuration));
            services.AddScoped<LocalizationPersistenceManager>();

            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            return services;
        }

        public static void GetDbContextOptions<T>(DbContextOptionsBuilder builder, IConfiguration configuration) where T : DbContext
        {
            //builder.EnableSensitiveDataLogging();
            var migrationsAssembly = typeof(T).GetTypeInfo().Assembly.GetName().Name;
            var useSqlServer = Convert.ToBoolean(configuration[$"{projectName}:UseSqlServer"] ?? "false");
            var dbConnString = useSqlServer
                ? configuration.GetConnectionString("DefaultConnection")
                : $"Filename={configuration.GetConnectionString("SqlLiteConnectionFileName")}";

            if (useSqlServer)
            {
                builder.UseSqlServer(dbConnString, options =>
                {
                    options.CommandTimeout(60);
                    options.MigrationsAssembly(migrationsAssembly);
                });
            }
            else if (Convert.ToBoolean(configuration[$"{projectName}:UsePostgresServer"] ?? "false"))
            {
                builder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"), options => options.MigrationsAssembly(migrationsAssembly));
            }
            else
            {
                builder.UseSqlite(dbConnString, options => options.MigrationsAssembly(migrationsAssembly));
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