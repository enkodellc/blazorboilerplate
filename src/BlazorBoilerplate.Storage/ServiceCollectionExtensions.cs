using System;
using System.Reflection;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Storage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterStorage(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<ApplicationDbContext>(builder => GetDbContextOptions(builder, Configuration)); // Look into the way we initialise the PB ways. Look at the old way they did this, with side effects on the builder. 
            services.AddScoped<IApplicationDbContext>(s => s.GetRequiredService<ApplicationDbContext>());
            
            services.AddTransient<IMessageStore, MessageStore>();
            services.AddTransient<IUserProfileStore, UserProfileStore>();
            services.AddTransient<IToDoStore, ToDoStore>();
            services.AddTransient<IApiLogStore, ApiLogStore>();
            
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
            
            return services;
        }

        public static void GetDbContextOptions(DbContextOptionsBuilder builder, IConfiguration configuration)
        {
            var migrationsAssembly = "BlazorBoilerplate.Storage";
            var useSqlServer = Convert.ToBoolean(configuration["BlazorBoilerplate:UseSqlServer"] ?? "false");
            var dbConnString = useSqlServer
                ? configuration.GetConnectionString("DefaultConnection")
                : $"Filename={configuration.GetConnectionString("SqlLiteConnectionFileName")}";

            if (useSqlServer)
            {
                builder.UseSqlServer(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
            else if (Convert.ToBoolean(configuration["BlazorBoilerplate:UsePostgresServer"] ?? "false"))
            {
                builder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"), sql => sql.MigrationsAssembly(migrationsAssembly));
            }
            else
            {
                builder.UseSqlite(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
        }

        /*public static DbContextOptionsBuilder GetDbContextOptions(this IConfiguration configuration)
        {
            var migrationsAssembly = "BlazorBoilerplate.Storage";
            var useSqlServer = Convert.ToBoolean(configuration["BlazorBoilerplate:UseSqlServer"] ?? "false");
            var dbConnString = useSqlServer
                ? configuration.GetConnectionString("DefaultConnection")
                : $"Filename={configuration.GetConnectionString("SqlLiteConnectionFileName")}";

            if (useSqlServer)
            {
                return new DbContextOptionsBuilder().UseSqlServer(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
            else if (Convert.ToBoolean(configuration["BlazorBoilerplate:UsePostgresServer"] ?? "false"))
            {
                return new DbContextOptionsBuilder().UseNpgsql(configuration.GetConnectionString("PostgresConnection"), sql => sql.MigrationsAssembly(migrationsAssembly));
            }
            else
            {
                return new DbContextOptionsBuilder().UseSqlite(dbConnString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
        }*/
    }
}