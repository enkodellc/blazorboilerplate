using BlazorBoilerplate.Infrastructure.Extensions;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Middleware
{
    //Logging  -> https://salslab.com/a/safely-logging-api-requests-and-responses-in-asp-net-core
    //Response -> https://www.c-sharpcorner.com/article/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consiste/
    public class APIResponseRequestLoggingMiddleware : BaseMiddleware
    {
        private IServiceScopeFactory _scopeFactory;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;
        private readonly bool _enableAPILogging;
        private readonly List<string> _ignorePaths;

        public APIResponseRequestLoggingMiddleware(RequestDelegate next,
            IConfiguration configuration,
            IStringLocalizer<Global> l,
            ILogger<APIResponseRequestLoggingMiddleware> logger) : base(next, l, logger)
        {
            _enableAPILogging = configuration.GetSection("BlazorBoilerplate:Api:Logging:Enabled").Get<bool>();
            _clearCacheHeadersDelegate = ClearCacheHeaders;
            _ignorePaths = configuration.GetSection("BlazorBoilerplate:Api:Logging:IgnorePaths").Get<List<string>>() ?? new List<string>();
        }

        public async Task Invoke(HttpContext httpContext, IServiceScopeFactory scopeFactory, UserManager<ApplicationUser> userManager)
        {
            _scopeFactory = scopeFactory;

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

                    using var responseBody = new MemoryStream();
                    try
                    {
                        string responseBodyContent = null;

                        var response = httpContext.Response;
                        //Breeze IQueryable paths here so it has the meta data to do its job
                        if (new string[] { "/api/localization", "/api/data", "/api/externalauth" }.Any(e => request.Path.StartsWithSegments(new PathString(e.ToLower()))))
                            await _next.Invoke(httpContext);
                        else
                        {
                            response.Body = responseBody;

                            await _next.Invoke(httpContext);

                            //wrap response in ApiResponse
                            if (httpContext.Response.StatusCode == Status200OK)
                            {
                                responseBodyContent = await FormatResponse(response);
                                await HandleSuccessRequestAsync(httpContext, responseBodyContent, Status200OK);
                            }
                            else
                                await HandleNotSuccessRequestAsync(httpContext, httpContext.Response.StatusCode);
                        }


                        stopWatch.Stop();

                        #region Log Request / Response
                        //Search the Ignore paths from appsettings to ignore the loggin of certian api paths
                        if (_enableAPILogging && _ignorePaths.All(e => !request.Path.StartsWithSegments(new PathString(e.ToLower()))))
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
                                    user);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("An Inner Middleware exception occurred on SafeLog: " + ex.Message);
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
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

        private Task HandleNotSuccessRequestAsync(HttpContext httpContext, int code)
        {
            ApiResponse apiResponse = new(code, ResponseMessage.GetDescription(code));
            httpContext.Response.StatusCode = code;

            return RewriteResponseAsApiResponse(httpContext, apiResponse);
        }

        private Task HandleSuccessRequestAsync(HttpContext httpContext, object body, int code)
        {
            var bodyText = !body.ToString().IsValidJson() ? ConvertToJSONString(body) : body.ToString();

            ApiResponse apiResponse = null;

            try
            {
                apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);
            }
            catch (Exception)
            {
            }

            var bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);

            if (apiResponse != null)
            {
                if (apiResponse.StatusCode == 0)
                    apiResponse.StatusCode = code;

                if ((apiResponse.Result == null) && string.IsNullOrEmpty(apiResponse.Message))
                    apiResponse = new ApiResponse(apiResponse.StatusCode, ResponseMessage.GetDescription(code), bodyContent);

            }
            else
                apiResponse = new ApiResponse(code, ResponseMessage.GetDescription(code), bodyContent);

            return RewriteResponseAsApiResponse(httpContext, apiResponse);
        }

        private static async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return $"{bodyAsText}";
        }

        private static async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return plainBodyText;
        }

        //TODO VS Studio Info / Warining message over the Disposable of the StreamReader
        //private async Task<string> FormatResponse(HttpResponse response)
        //{
        //    using (StreamReader reader = new StreamReader(response.Body))
        //    {
        //        response.Body.Seek(0, SeekOrigin.Begin);
        //        var plainBodyText = await reader.ReadToEndAsync();
        //        response.Body.Seek(0, SeekOrigin.Begin);
        //        return plainBodyText;
        //    }
        //}

        private static string ConvertToJSONString(object rawJSON)
        {
            return JsonConvert.SerializeObject(rawJSON, JSONSettings());
        }

        private static bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger");
        }

        private static JsonSerializerSettings JSONSettings()
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
            if (requestBody.Length > 256)
                requestBody = $"(Truncated to 200 chars) {requestBody.Substring(0, 200)}";

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
                responseBody = $"(Truncated to 200 chars) {responseBody.Substring(0, 200)}";

            if (queryString.Length > 256)
                queryString = $"(Truncated to 200 chars) {queryString.Substring(0, 200)}";


            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.ApiLogs.Add(new ApiLogItem
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
                ApplicationUserId = user?.Id
            });

            await dbContext.SaveChangesAsync();
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
