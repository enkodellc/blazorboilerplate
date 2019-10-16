using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Blazor.Http;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Client.Services.Implementations;
using BlazorBoilerplate.Client.States;
using MatBlazor;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;

namespace BlazorBoilerplate.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            WebAssemblyHttpMessageHandler.DefaultCredentials = FetchCredentialsOption.Include;
            app.UseLoadingBar();
            app.AddComponent<App>("app");
        }
    }
}
