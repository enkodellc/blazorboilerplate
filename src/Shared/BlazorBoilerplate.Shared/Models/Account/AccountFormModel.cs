namespace BlazorBoilerplate.Shared.Models.Account
{
    public class AccountFormModel
    {
        private string returnUrl;

        public bool RememberMe { get; set; }

        public string ReturnUrl
        {
            get => returnUrl ?? "/";
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.StartsWith("http"))
                        value = new Uri(value).LocalPath;

                    if (!value.StartsWith("/"))
                        value = $"/{value}";
                }

                returnUrl = value;
            }
        }

        public string __RequestVerificationToken { get; set; }
    }
}
