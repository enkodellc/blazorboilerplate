using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BlazorBoilerplate.Theme.Material.TagHelpers
{
    public class ThemeTagHelperComponent : TagHelperComponent
    {
        public override int Order => 1;
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml(@$"
<link rel=""shortcut icon"" type=""image/x-icon"" href=""{Module.ContentPath}/images/favicon.ico"">
<link rel=""icon"" type=""image/x-icon"" href=""{Module.ContentPath}/images/favicon.ico"">
<link href=""https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css"" rel=""stylesheet"" />
<link href=""https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap"" rel=""stylesheet"" />
<link href=""_content/MudBlazor/MudBlazor.min.css"" rel=""stylesheet"" />
<link href=""//cdn.quilljs.com/1.3.6/quill.snow.css"" rel=""stylesheet"">
<link href=""//cdn.quilljs.com/1.3.6/quill.bubble.css"" rel=""stylesheet"">
<link href=""{Module.ContentPath}/css/site.css"" rel=""stylesheet"" />
<link href=""{Module.ContentPath}/{Module.Path}.bundle.scp.css"" rel=""stylesheet"" />");
            }
            else if (string.Equals(context.TagName, "app", StringComparison.OrdinalIgnoreCase))
            {
                output.PostElement.AppendHtml(@$"
<script src=""_content/MudBlazor/MudBlazor.min.js""></script>
<script src=""https://cdn.quilljs.com/1.3.6/quill.js""></script>
<script src=""_content/Blazored.TextEditor/quill-blot-formatter.min.js""></script>
<script src=""_content/Blazored.TextEditor/Blazored-BlazorQuill.js""></script>");
            }

            return Task.CompletedTask;
        }
    }
}
