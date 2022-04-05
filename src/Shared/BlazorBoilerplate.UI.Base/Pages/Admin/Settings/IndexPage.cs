using BlazorBoilerplate.Shared.Dto.Db;

namespace BlazorBoilerplate.UI.Base.Pages.Admin.Settings
{
    public class IndexPage : SettingsBase
    {
        protected string[] BlazorRuntimes { get; set; } = ((BlazorRuntime[])Enum.GetValues(typeof(BlazorRuntime))).Select(i => i.ToString()).ToArray();

        protected override async Task OnInitializedAsync()
        {
            await LoadSettings("MainConfiguration_");
        }
    }
}
