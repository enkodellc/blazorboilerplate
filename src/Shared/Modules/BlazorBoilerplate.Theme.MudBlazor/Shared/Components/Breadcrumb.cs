using Microsoft.AspNetCore.Components;
using System;

namespace BlazorBoilerplate.Theme.Material.Shared.Components
{
    public partial class Breadcrumb : ComponentBase, IDisposable
    {
        [CascadingParameter]
        protected internal Breadcrumbs Parent { get; set; }

        [Parameter]
        public string Link { get; set; }

        [Parameter]
        public string Title { get; set; }

        public Breadcrumb()
        {

        }

        public Breadcrumb(string link, string title)
        {
            Link = link;
            Title = title;
        }

        protected override void OnInitialized()
        {
            Parent.Items.Add(this);
        }

        public void Dispose()
        {
            Parent.Items.Remove(this);
        }
    }
}
