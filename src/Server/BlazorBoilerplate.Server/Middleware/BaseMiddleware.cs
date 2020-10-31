using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Localizer;
using Breeze.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ObjectCloner.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Middleware
{
    public abstract class BaseMiddleware
    {
        protected ILogger<BaseMiddleware> _logger;
        protected IStringLocalizer<Global> L;

        //https://trailheadtechnology.com/aspnetcore-multi-tenant-tips-and-tricks/
        protected readonly RequestDelegate _next;

        protected BaseMiddleware(RequestDelegate next, IStringLocalizer<Global> l, ILogger<BaseMiddleware> logger)
        {
            _next = next;
            L = l;
            _logger = logger;
        }

        protected Task RewriteResponseAsApiResponse(HttpContext httpContext, ApiResponse apiResponse)
        {
            var headers = httpContext.Response.Headers.DeepClone();
            httpContext.Response.Clear();

            foreach (var h in headers.Where(i => !i.Key.StartsWith("Content")))
                httpContext.Response.Headers.Add(h.Key, h.Value);

            httpContext.Response.ContentType = "application/json";

            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
        }

        protected async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            string msg = exception.GetBaseException().StackTrace;
            string userMsg = L["Operation Failed"];
            int code = Status500InternalServerError;

            if (exception is EntityErrorsException)
            {
                return;
            }
            else if (exception is UnauthorizedAccessException)
            {
                userMsg = msg = L["UnauthorizedAccess"];
                code = Status401Unauthorized;
            }
            else if (exception is DomainException)
            {
                userMsg = msg = ((DomainException)exception).Description;
            }

            _logger.LogError($"Api Exception: {msg}");

            httpContext.Response.StatusCode = code;

            await RewriteResponseAsApiResponse(httpContext, new ApiResponse(Status500InternalServerError, userMsg));
        }

    }
}
