using BlazorBoilerplate.Server.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BlazorBoilerplate.Server.Controllers
{
    public class BaseController : Controller
    {
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
