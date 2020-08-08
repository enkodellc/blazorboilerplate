using BlazorBoilerplate.Localization;
using BlazorBoilerplate.Shared.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class UserViewModel : BaseDto
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [StringLength(64, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorInvalidLength", MinimumLength = 2)]
        [RegularExpression(@"[^\s]+", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "SpacesNotPermitted")]
        [Display(Name = "UserName", ResourceType = typeof(Strings))]
        public string UserName { get; set; }

        public string TenantId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "EmailInvalid")]
        [Display(Name = "Email")]
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