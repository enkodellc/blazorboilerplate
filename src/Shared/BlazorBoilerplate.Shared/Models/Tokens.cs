namespace BlazorBoilerplate.Shared.Models
{
    public class Tokens
    {
        public DateTimeOffset AccessTokenExpiration { get; set; }
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
