using BlazorBoilerplate.Server.Services;
using Serilog;

namespace BlazorBoilerplate.Server
{
    public class Program
    {
        public static SemaphoreSlim Sync { get; private set; }
        public static int Main(string[] args)
        {
            Sync = new SemaphoreSlim(0, 1);

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting BlazorBoilerplate web server host");
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
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<EmailService>();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build());
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog();
    }
}
