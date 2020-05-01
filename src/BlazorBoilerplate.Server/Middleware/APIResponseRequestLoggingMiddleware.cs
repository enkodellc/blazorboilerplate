using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Server.Middleware.Extensions;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using System.Text.Json; //Does not work for this middleware, at least as in preview
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Middleware
{
    //Logging  -> https://salslab.com/a/safely-logging-api-requests-and-responses-in-asp-net-core
    //Response -> https://www.c-sharpcorner.com/article/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consiste/
    //Latest: https://github.com/proudmonkey/AutoWrapper
    public class APIResponseRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        ILogger<APIResponseRequestLoggingMiddleware> _logger;
        private IApiLogManager _apiLogManager;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;
        private readonly bool _enableAPILogging;
        private List<string> _ignorePaths = new List<string>();

        public APIResponseRequestLoggingMiddleware(RequestDelegate next, bool enableAPILogging, IConfiguration configuration)
        {
            _next = next;
            _enableAPILogging = enableAPILogging;
            _clearCacheHeadersDelegate = ClearCacheHeaders;
            _ignorePaths = configuration.GetSection("BlazorBoilerplate:ApiLogging:IgnorePaths").Get<List<string>>();
        }

        public async Task Invoke(HttpContext httpContext, IApiLogManager apiLogManager, ILogger<APIResponseRequestLoggingMiddleware> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _apiLogManager = apiLogManager;

            try
            {
                var request = httpContext.Request;
                if (IsSwagger(httpContext) || !request.Path.StartsWithSegments(new PathString("/api")))
                {
                    await _next(httpContext);
                }
                else
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();
                    var requestTime = DateTime.UtcNow;

                    var formattedRequest = await FormatRequest(request);
                    var originalBodyStream = httpContext.Response.Body;

                    using (var responseBody = new MemoryStream())
                    {
                        httpContext.Response.Body = responseBody;

                        try
                        {
                            var response = httpContext.Response;
                            response.Body = responseBody;
                            await _next.Invoke(httpContext);

                            string responseBodyContent = null;

                            if (httpContext.Response.StatusCode == Status200OK)
                            {
                                responseBodyContent = await FormatResponse(response);
                                await HandleSuccessRequestAsync(httpContext, responseBodyContent, Status200OK);
                            }
                            else
                            {
                                await HandleNotSuccessRequestAsync(httpContext, httpContext.Response.StatusCode);
                            }

                            httpContext.Response.ContentLength = responseBody.Length;

                            stopWatch.Stop();

                            #region Log Request / Response
                            //Search the Ignore paths from appsettings to ignore the loggin of certian api paths
                            if (_enableAPILogging && (_ignorePaths.Any(e => !request.Path.StartsWithSegments(new PathString(e.ToLower())))))
                            {
                                try
                                {
                                    await responseBody.CopyToAsync(originalBodyStream);

                                    //User id = "sub" y default
                                    ApplicationUser user = httpContext.User.Identity.IsAuthenticated
                                            ? await userManager.FindByIdAsync(httpContext.User.Claims.Where(c => c.Type == JwtClaimTypes.Subject).First().Value)
                                            : null;

                                    await SafeLog(requestTime,
                                        stopWatch.ElapsedMilliseconds,
                                        response.StatusCode,
                                        request.Method,
                                        request.Path,
                                        request.QueryString.ToString(),
                                        formattedRequest,
                                        responseBodyContent,
                                        httpContext.Connection.RemoteIpAddress.ToString(),
                                        user
                                        );
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning("An Inner Middleware exception occurred on SafeLog: " + ex.Message);
                                }
                            }
                            #endregion
                        }
                        catch (System.Exception ex)
                        {

                            _logger.LogWarning("An Inner Middleware exception occurred: " + ex.Message);
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

        private async Task HandleExceptionAsync(HttpContext httpContext, System.Exception exception)
        {
            _logger.LogError("Api Exception:", exception);

            ApiError apiError = null;
            ApiResponse apiResponse = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                apiError = new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ex.Errors)
                {
                    ValidationErrors = ex.Errors,
                    ReferenceErrorCode = ex.ReferenceErrorCode,
                    ReferenceDocumentLink = ex.ReferenceDocumentLink
                };
                code = ex.StatusCode;
                httpContext.Response.StatusCode = code;

            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");
                code = Status401Unauthorized;
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

                apiError = new ApiError(msg)
                {
                    Details = stack
                };
                code = Status500InternalServerError;
                httpContext.Response.StatusCode = code;
            }

            httpContext.Response.ContentType = "application/json";

            apiResponse = new ApiResponse(code, ResponseMessageEnum.Exception.GetDescription(), null, apiError);

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
        }

        private Task HandleNotSuccessRequestAsync(HttpContext httpContext, int code)
        {
            ApiError apiError;

            if (code == Status404NotFound)
            {
                apiError = new ApiError(ResponseMessageEnum.NotFound.GetDescription());
            }
            else if (code == Status204NoContent)
            {
                apiError = new ApiError(ResponseMessageEnum.NotContent.GetDescription());
            }
            else if (code == Status405MethodNotAllowed)
            {
                apiError = new ApiError(ResponseMessageEnum.MethodNotAllowed.GetDescription());
            }
            else if (code == Status401Unauthorized)
            {
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
            }
            else
            {
                apiError = new ApiError(ResponseMessageEnum.Unknown.GetDescription());
            }

            ApiResponse apiResponse = new ApiResponse(code, apiError);
            httpContext.Response.StatusCode = code;
            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
        }

        private Task HandleSuccessRequestAsync(HttpContext httpContext, object body, int code)
        {
            string jsonString = string.Empty;
            var bodyText = !body.ToString().IsValidJson() ? ConvertToJSONString(body) : body.ToString();

            ApiResponse apiResponse = null;

            if (!body.ToString().IsValidJson())
            {
                return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
            }
            else
            {
                bodyText = body.ToString();
            }

            //TODO Review the code below as it might not be necessary
            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type = bodyContent?.GetType();

            // Check to see if body is already an ApiResponse Class type
            if (type.Equals(typeof(Newtonsoft.Json.Linq.JObject)))
            {
                apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);
                if (apiResponse.StatusCode == 0)
                {
                    apiResponse.StatusCode = code;
                }

                if ((apiResponse.Result != null) || (!string.IsNullOrEmpty(apiResponse.Message)))
                {
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                }
                else
                {
                    apiResponse = new ApiResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, null);
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                }
            }
            else
            {
                apiResponse = new ApiResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, null);
                jsonString = JsonConvert.SerializeObject(apiResponse);
            }

            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync(jsonString);
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return $"{request.Method} {request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return plainBodyText;
        }

        private string ConvertToJSONString(int code, object content)
        {
            return JsonConvert.SerializeObject(new ApiResponse(code, ResponseMessageEnum.Success.GetDescription(), content, null, string.Empty), JSONSettings());
        }
        private string ConvertToJSONString(ApiResponse apiResponse)
        {
            return JsonConvert.SerializeObject(apiResponse, JSONSettings());
        }
        private string ConvertToJSONString(object rawJSON)
        {
            return JsonConvert.SerializeObject(rawJSON, JSONSettings());
        }

        private bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger");
        }

        private JsonSerializerSettings JSONSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }
        private async Task SafeLog(DateTime requestTime,
                            long responseMillis,
                            int statusCode,
                            string method,
                            string path,
                            string queryString,
                            string requestBody,
                            string responseBody,
                            string ipAddress,
                            ApplicationUser user)
        {
            // Do not log these events login, logout, getuserinfo...
            if ((path.ToLower().StartsWith("/api/account/")) ||
                (path.ToLower().StartsWith("/api/UserProfile/")))
            {
                return;
            }

            if (requestBody.Length > 256)
            {
                requestBody = $"(Truncated to 200 chars) {requestBody.Substring(0, 200)}";
            }

            // If the response body was an ApiResponse we should just save the Result object
            if (responseBody != null && responseBody.Contains("\"result\":"))
            {
                try
                {
                    ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
                    responseBody = Regex.Replace(apiResponse.Result.ToString(), @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
                }
                catch { }
            }

            if (responseBody != null && responseBody.Length > 256)
            {
                responseBody = $"(Truncated to 200 chars) {responseBody.Substring(0, 200)}";
            }

            if (queryString.Length > 256)
            {
                queryString = $"(Truncated to 200 chars) {queryString.Substring(0, 200)}";
            }

            // Pass in the context to resolve the instance, and save to a store?
            await _apiLogManager.Log(new ApiLogItem
            {
                RequestTime = requestTime,
                ResponseMillis = responseMillis,
                StatusCode = statusCode,
                Method = method,
                Path = path,
                QueryString = queryString,
                RequestBody = requestBody,
                ResponseBody = responseBody ?? String.Empty,
                IPAddress = ipAddress,
                ApplicationUserId = user == null ? Guid.Empty : user.Id
            });
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
