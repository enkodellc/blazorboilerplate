using BlazorBoilerplate.Server.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Middleware
{
    public class UserSessionMiddleware
    {
        //https://trailheadtechnology.com/aspnetcore-multi-tenant-tips-and-tricks/
        private readonly RequestDelegate _next;
        public UserSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext, IUserSession userSession)
        {
            try
            {
                var request = httpContext.Request;
                //if (!request.Path.StartsWithSegments(new PathString("/api")))
                //{
                //    await _next.Invoke(httpContext);
                //}
                //else
                //{
                    // Call the next delegate/middleware in the pipeline
                    await _next.Invoke(httpContext);

                    if (httpContext.User.Identity.IsAuthenticated)
                    //if (httpContext.User.Identities.Any(id => id.IsAuthenticated))
                    {
                        userSession.UserId = new Guid(httpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
                        userSession.UserName = httpContext.User.Identity.Name;
                        userSession.TenantId = -1; // ClaimsHelper.GetClaim<int>(context.User, "tenantid");
                        userSession.Roles = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                    }
             //   }
            }
            catch(Exception ex)
            {
                string test = ex.Message;
            }
        }
    }
}
