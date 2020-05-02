using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Demo
{
    public class ThemeTagHelperComponent : TagHelperComponent
    {
        public override int Order => 2;
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var path = GetType().Namespace;

            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml(@$"
<script src=""_content/{path}/javascript/signalr.min.js""></script>
<script src=""_content/{path}/javascript/chatClient.js""></script>");
            }

            return Task.CompletedTask;
        }
    }
}
