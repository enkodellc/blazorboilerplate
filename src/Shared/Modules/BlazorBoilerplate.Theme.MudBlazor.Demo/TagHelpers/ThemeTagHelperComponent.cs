using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BlazorBoilerplate.Theme.Material.Demo.TagHelpers
{
    public class ThemeTagHelperComponent : TagHelperComponent
    {
        public override int Order => 1;
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var path = typeof(Module).Namespace.Replace("Material", "MudBlazor");

            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml(@$"
<link href=""_content/{path}/css/site.css"" rel=""stylesheet"" />
<link href=""_content/{path}/{path}.bundle.scp.css"" rel=""stylesheet"" />");
            }

            return Task.CompletedTask;
        }
    }
}
