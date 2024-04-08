
namespace BlazorBoilerplate.Infrastructure.AuthorizationDefinitions
{
    public static class Policies
    {
        public const string IsAdmin = "IsAdmin";
        public const string IsUser = "IsUser";
        public const string IsMyEmailDomain = "IsMyEmailDomain";
        public const string TwoFactorEnabled = "TwoFactorEnabled";
    }
}
