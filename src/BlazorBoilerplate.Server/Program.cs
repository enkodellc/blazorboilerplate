using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.Hosting;

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

                // Per: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host
                //    The Web Host is no longer recommended for web apps and remains 
                //    available only for backward compatibility.
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "BlazorBoilerplate Host terminated unexpectedly");
                return 1;
            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args).Build())
                    .UseStartup<Startup>()
                    .UseSerilog();
                });
    };
}
