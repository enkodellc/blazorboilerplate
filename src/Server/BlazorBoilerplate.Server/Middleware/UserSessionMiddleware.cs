using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Middleware
{
    public class UserSessionMiddleware : BaseMiddleware
    {
        public UserSessionMiddleware(RequestDelegate next, IStringLocalizer<Global> l, ILogger<UserSessionMiddleware> logger) : base(next, l, logger)
        { }

        public async Task InvokeAsync(HttpContext httpContext, IUserSession userSession)
        {
            try
            {
                var request = httpContext.Request;

                //First setup the userSession, then call next midleware
                if (httpContext.User.Identity.IsAuthenticated)
                {
                    userSession.UserId = new Guid(httpContext.User.Claims.Where(c => c.Type == JwtClaimTypes.Subject).First().Value);
                    userSession.UserName = httpContext.User.Identity.Name;
                    
                    userSession.Roles = httpContext.User.Claims.Where(c => c.Type == JwtClaimTypes.Role).Select(c => c.Value).ToList();
                    userSession.ExposedClaims = httpContext.User.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList();
                }

                // Call the next delegate/middleware in the pipeline
                await _next.Invoke(httpContext);
            }
            catch (Exception ex)
            {
                // We can't do anything if the response has already started, just abort.
                if (httpContext.Response.HasStarted)
                {
                    _logger.LogWarning("A Middleware exception occurred, but response has already started!");
                    throw;
                }

                await HandleExceptionAsync(httpContext, ex);
                throw;
            }
        }
    }
}