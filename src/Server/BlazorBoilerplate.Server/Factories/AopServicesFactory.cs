using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;

namespace BlazorBoilerplate.Server.Factories
{
    public static class AopServicesFactory
    {
        public static readonly ServiceProvider ServiceProvider;

        static AopServicesFactory()
        {
            ServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddLocalization()
                .Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture(Settings.SupportedCultures[0]);
                    options.AddSupportedCultures(Settings.SupportedCultures);
                    options.AddSupportedUICultures(Settings.SupportedCultures);
                })
                .AddTransient<ApiResponseExceptionAspect>()
                .AddTransient<LogExceptionAspect>()
                .AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory())
                .BuildServiceProvider();
        }

        public static object GetInstance(Type type) => ServiceProvider.GetRequiredService(type);
    }
}
