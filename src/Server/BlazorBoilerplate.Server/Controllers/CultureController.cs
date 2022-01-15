using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Globalization;

namespace BlazorBoilerplate.Server.Controllers
{
    [OpenApiIgnore]
    [Route("[controller]/[action]")]
    public class CultureController : Controller
    {
        public IActionResult SetCulture(string culture, string redirectUri)
        {
            if (culture != null)
            {
                HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture)),
                        new CookieOptions() { Expires = DateTimeOffset.Now.AddDays(30) });
            }

            return LocalRedirect(redirectUri);
        }
    }
}
