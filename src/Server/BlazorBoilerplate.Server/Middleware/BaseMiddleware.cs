using BlazorBoilerplate.Infrastructure.Server.Models;
using Breeze.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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
            var jsonString = JsonConvert.SerializeObject(apiResponse);
            httpContext.Response.Clear();
            httpContext.Response.ContentType = "application/json";

            return httpContext.Response.WriteAsync(jsonString);
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
