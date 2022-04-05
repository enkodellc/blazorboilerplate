using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class UserViewModel : BaseDto
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool HasPassword { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool HasAuthenticator { get; set; }
        public List<KeyValuePair<string, string>> Logins { get; set; }
        public bool BrowserRemembered { get; set; }
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        public string[] RecoveryCodes { get; set; }
        public int CountRecoveryCodes { get; set; }
        public List<string> Roles { get; set; }
        public List<KeyValuePair<string, string>> ExposedClaims { get; set; }
    }
}