using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class LoginWith2faModel : AccountFormModel
    {
        [Display(Name = "RememberBrowser")]
        public bool RememberMachine { get; set; }
    }
}
