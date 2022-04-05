using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Storage;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BlazorBoilerplate.Storage
{
    public static class ServiceCollectionExtensions
    {
#if DEBUG
        public static readonly ILoggerFactory factory = LoggerFactory.Create(builder => { builder.AddDebug(); });
#endif

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
#if DEBUG
            builder.UseLoggerFactory(factory).EnableSensitiveDataLogging();
#endif
            var migrationsAssembly = typeof(T).GetTypeInfo().Assembly.GetName().Name;
            var useSqlServer = !Convert.ToBoolean(configuration[$"{projectName}:UsePostgresServer"] ?? "false");

            if (useSqlServer)
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentNullException("The DefaultConnection was not found.");

                if (!connectionString.ToLower().Contains("multipleactiveresultsets=true"))
                    throw new ArgumentException("When Sql Server is in use the DefaultConnection must contain: MultipleActiveResultSets=true");

                builder.UseSqlServer(connectionString, options =>
                {
                    options.CommandTimeout(60);
                    options.MigrationsAssembly(migrationsAssembly);
                });
            }
            else
                builder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"), options => options.MigrationsAssembly(migrationsAssembly));
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