using BlazorBoilerplate.Server.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace BlazorBoilerplate.Server.Controllers
{
    public class BaseController : Controller
    {
        protected const string AuthSchemes =
            "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme; //Cookie + Token authentication
        protected Guid GetUserId()
        {
            return User.GetUserId();
        }

        protected string GetClientId()
        {
            return User.GetClientId();
        }
    }
}
