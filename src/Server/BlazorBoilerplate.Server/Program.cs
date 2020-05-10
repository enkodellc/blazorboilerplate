using System;
using BlazorBoilerplate.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BlazorBoilerplate.Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                //IdentityServer4 seed should be happening here but because of this bug https://github.com/aspnet/AspNetCore/issues/12349
                //the seeding is not implemented here.
                Log.Information("Starting BlazorBoilerplate web server host");
                BuildWebHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "BlazorBoilerplate Host terminated unexpectedly");
                return 1;
            }
        }

        public static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new DryIocServiceProviderFactory())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                })
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder
                        .UseConfiguration(new ConfigurationBuilder()
                            .AddCommandLine(args)
                            .Build())

                        .UseStartup<Startup>()
                        .UseSerilog();
                })
                .Build();
    }
}
