using BlazorBoilerplate.Infrastructure.Server.Models;
using Breeze.Persistence;
using Microsoft.AspNetCore.Http;
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

        //https://trailheadtechnology.com/aspnetcore-multi-tenant-tips-and-tricks/
        protected readonly RequestDelegate _next;

        protected BaseMiddleware(RequestDelegate next)
        {
            _next = next;
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
            _logger.LogError("Api Exception:", exception.GetBaseException());

            ApiError apiError;
            int code;

            if (exception is EntityErrorsException)
            {
                return;
            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");
                code = Status401Unauthorized;
                httpContext.Response.StatusCode = code;
            }
            else
            {
                apiError = new ApiError(exception.GetBaseException().Message)
                {
                    Details = exception.StackTrace
                };

                code = Status500InternalServerError;
                httpContext.Response.StatusCode = code;
            }

            ApiResponse apiResponse = new ApiResponse(code, ResponseMessage.GetDescription(code), null, apiError);

            await RewriteResponseAsApiResponse(httpContext, apiResponse);
        }

    }
}
