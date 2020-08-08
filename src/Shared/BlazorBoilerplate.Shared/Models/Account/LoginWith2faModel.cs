using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class LoginWith2faModel : AccountFormModel
    {
        [Display(Name = "RememberBrowser", ResourceType = typeof(Strings))]
        public bool RememberMachine { get; set; }
    }
}
