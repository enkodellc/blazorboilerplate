using SourceGenerators;

namespace BlazorBoilerplate.Shared.Models
{
    public partial class UserFilter : QueryParameters
    {
        [AutoNotify]
        private string _search;

        [AutoNotify]
        private bool? _emailConfirmed;

        [AutoNotify]
        private bool? _phoneNumberConfirmed;
    }
}