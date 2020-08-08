using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class LoginInputModel : AccountFormModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [StringLength(64, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorInvalidLength", MinimumLength = 2)]
        [RegularExpression(@"[^\s]+", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "SpacesNotPermitted")]
        [Display(Name = "UserName", ResourceType = typeof(Strings))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        [StringLength(100, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorInvalidLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
