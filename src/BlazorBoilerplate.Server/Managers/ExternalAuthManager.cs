using System.Threading.Tasks;
using AutoMapper.Configuration;
using BlazorBoilerplate.Server.Controllers;
using BlazorBoilerplate.Shared.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplate.Server.Managers
{
    // TODO: The methods are very tightly coupled with the controllers. Will re-visit. 
    public class ExternalAuthManager : IExternalAuthManager
    {
        public ExternalAuthManager(IAccountManager accountManager, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            
        }
        
        public Task<IActionResult> Challenge(string provider)
        {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> ExternalSignIn()
        {
            throw new System.NotImplementedException();
        }
    }
}