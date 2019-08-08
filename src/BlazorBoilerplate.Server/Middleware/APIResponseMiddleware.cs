using System;
using System.IO;
using System.Net;
//using System.Text.Json; //Does not work for this middleware, at least as in preview
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Middleware.Extensions;

namespace BlazorBoilerplate.Server.Middleware
{
    //https://www.c-sharpcorner.com/article/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consiste/
    public class APIResponseMiddleware
    {
        private readonly RequestDelegate _next;
        ILogger<APIResponseMiddleware> _logger;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;

        public APIResponseMiddleware(RequestDelegate next)
        {
            _next = next;
            _clearCacheHeadersDelegate = ClearCacheHeaders;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<APIResponseMiddleware> logger)
        {
            _logger = logger;

            if (IsSwagger(httpContext) || !httpContext.Request.Path.StartsWithSegments(new PathString("/api")))
            {
                await _next(httpContext);
            }
            else
            {
                var originalBodyStream = httpContext.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    httpContext.Response.Body = responseBody;

                    try
                    {
                        await _next.Invoke(httpContext);

                        if (httpContext.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            var body = await FormatResponse(httpContext.Response);
                            await HandleSuccessRequestAsync(httpContext, body, httpContext.Response.StatusCode);
                        }
                        else
                        {
                            await HandleNotSuccessRequestAsync(httpContext, httpContext.Response.StatusCode);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        await HandleExceptionAsync(httpContext, ex);
                    }
                    finally
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
            }
        }


        private async Task HandleExceptionAsync(HttpContext httpContext, System.Exception exception)
        {
            _logger.LogError("Api Exception:", exception);

            ApiError apiError = null;
            APIResponse apiResponse = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                apiError = new ApiError(ex.Message);
                apiError.ValidationErrors = ex.Errors;
                apiError.ReferenceErrorCode = ex.ReferenceErrorCode;
                apiError.ReferenceDocumentLink = ex.ReferenceDocumentLink;
                code = ex.StatusCode;
                httpContext.Response.StatusCode = code;

            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");
                code = (int)HttpStatusCode.Unauthorized;
                httpContext.Response.StatusCode = code;
            }
            else
            {
#if !DEBUG
                var msg = "An unhandled error occurred.";
                string stack = null;
#else
                var msg = exception.GetBaseException().Message;
                string stack = exception.StackTrace;
#endif

                apiError = new ApiError(msg);
                apiError.Details = stack;
                code = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.StatusCode = code;
            }

            httpContext.Response.ContentType = "application/json";

            apiResponse = new APIResponse(code, ResponseMessageEnum.Exception.GetDescription(), null, apiError);

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
        }

        private static Task HandleNotSuccessRequestAsync(HttpContext httpContext, int code)
        {
            httpContext.Response.ContentType = "application/json";

            ApiError apiError = null;
            APIResponse apiResponse = null;

            if (code == (int)HttpStatusCode.NotFound)
            {
                apiError = new ApiError("The specified URI does not exist. Please verify and try again.");
            }
            else if (code == (int)HttpStatusCode.NoContent)
            {
                apiError = new ApiError("The specified URI does not contain any content.");
            }
            else
            {
                apiError = new ApiError("Your request cannot be processed. Please contact a support.");
            }

            apiResponse = new APIResponse(code, ResponseMessageEnum.Failure.GetDescription(), null, apiError);
            httpContext.Response.StatusCode = code;
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
        }

        private static Task HandleSuccessRequestAsync(HttpContext httpContext, object body, int code)
        {
            httpContext.Response.ContentType = "application/json";
            string jsonString, bodyText = string.Empty;
            APIResponse apiResponse = null;

            if (!body.ToString().IsValidJson())
            {
                return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
            }
            else
            {
                bodyText = body.ToString();
            }

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type = bodyContent?.GetType();

            // Check to see if body is already an APIResponse Class type
            if (type.Equals(typeof(Newtonsoft.Json.Linq.JObject)))
            {
                apiResponse = JsonConvert.DeserializeObject<APIResponse>(bodyText);
                if (apiResponse.StatusCode != code) 
                {
                    apiResponse.StatusCode = code;
                }

                if ( (apiResponse.Result != null) || (!string.IsNullOrEmpty(apiResponse.Message)) )
                {
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                }
                else
                {
                    apiResponse = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, null);
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                }
            }
            else
            {
                apiResponse = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, null);
                jsonString = JsonConvert.SerializeObject(apiResponse);
            }

            return httpContext.Response.WriteAsync(jsonString);
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return plainBodyText;
        }

        private bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger");

        }

        private Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;

            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);

            return Task.CompletedTask;
        }
    }
}
