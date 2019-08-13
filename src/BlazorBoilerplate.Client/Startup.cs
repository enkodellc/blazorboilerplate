using Microsoft.AspNetCore.Blazor.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Client.Services.Implementations;
using BlazorBoilerplate.Client.States;
using MatBlazor;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorizationCore();
            services.AddScoped<IdentityAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
            services.AddScoped<IAuthorizeApi, AuthorizeApi>();
            services.AddLoadingBar();
            services.Add(new ServiceDescriptor(typeof(IUserProfileApi), typeof(UserProfileApi), ServiceLifetime.Scoped));
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
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            WebAssemblyHttpMessageHandler.DefaultCredentials = FetchCredentialsOption.Include;
            app.UseLoadingBar();
            app.AddComponent<App>("app");
        }
    }
}
