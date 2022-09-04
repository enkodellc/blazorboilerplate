using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorBoilerplate.Server.Pages
{
    public class HostModel : PageModel
    {
        public HostModel(ITenantSettings<MainConfiguration> mainConfiguration)
        {
            AppState.Runtime = mainConfiguration.Value.Runtime;
        }
    }
}
