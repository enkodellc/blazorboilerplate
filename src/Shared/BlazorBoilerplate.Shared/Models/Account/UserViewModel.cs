using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using IntlTelInputBlazor;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class UserViewModel : RegisterViewModel
    {
        private IntlTel companyIntTelNumber;

        public bool IsAuthenticated { get; set; }
        public Guid? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => (!string.IsNullOrEmpty(LastName) || !string.IsNullOrEmpty(LastName)) ? $"{FirstName} {LastName}" : Email;
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
        public Dictionary<UserFeatures, bool> UserFeatures { get; private set; } = Enum.GetValues<UserFeatures>().ToDictionary(k => k, v => false);
        public DateTime? ExpirationDate { get; set; }

        public string CompanyName { get; set; }
        public double? CompanyLongitude { get; set; }
        public double? CompanyLatitude { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyProvince { get; set; }
        public string CompanyZipCode { get; set; }
        public string CompanyCountryCode { get; set; }
        public string CompanyVatIn { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public IntlTel CompanyIntTelNumber
        {
            get => string.IsNullOrWhiteSpace(CompanyPhoneNumber) ? companyIntTelNumber : new() { Number = CompanyPhoneNumber, IsValid = true };
            set
            {
                companyIntTelNumber = value;

                if (value?.IsValid == true)
                    CompanyPhoneNumber = value.Number;
                else
                {
                    CompanyPhoneNumber = null;

                    if (value?.Number == string.Empty)
                        companyIntTelNumber = null;
                }
            }
        }
    }
}