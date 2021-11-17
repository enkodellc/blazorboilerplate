using BlazorBoilerplate.UI.Base.Shared.Layouts;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorBoilerplate.UI.Base.Shared.Components
{
    public partial class TopSection : ComponentBase, IDisposable
    {
        [CascadingParameter]
        public RootLayout RootLayout { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void OnInitialized()
        {
            if (RootLayout != null) 
                RootLayout.SetTopSection(this);

            base.OnInitialized();
        }

        protected override bool ShouldRender()
        {
            var shouldRender = base.ShouldRender();
            if (shouldRender && RootLayout != null)
            {
                RootLayout.Update();
            }
            return base.ShouldRender();
        }

        public void Dispose()
        {
            RootLayout?.SetTopSection(null);
        }
    }
}
