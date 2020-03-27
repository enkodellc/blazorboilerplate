using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.Dto.ExternalAuth;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.DataModels;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IExternalAuthManager _externalAuthManager;

        public ExternalAuthController(IExternalAuthManager externalAuthManager)
        {
            _externalAuthManager = externalAuthManager;
        }
        
        [HttpGet("challenge/{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> Challenge(string provider)
        {
            try
            {
                var (authProps, schemaName) = await _externalAuthManager.Challenge(Url.RouteUrl("ExternalSignIn"), provider);

                return Challenge(authProps, schemaName);
            }
            catch (ArgumentNullException ex)
            {
                return Redirect(ex.Message);
            }
        }


        [HttpGet("ExternalSignIn", Name = "ExternalSignIn")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalSignIn()
        => Redirect(await _externalAuthManager.ExternalSignIn(HttpContext));

    }
}
