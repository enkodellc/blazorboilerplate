using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Services;

namespace BlazorBoilerplate.Server.Middleware
{
    //https://salslab.com/a/safely-logging-api-requests-and-responses-in-asp-net-core
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private ApiLogService _apiLogService;
        ILogger<ApiLoggingMiddleware> _logger;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;

        public ApiLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _clearCacheHeadersDelegate = ClearCacheHeaders;
        }

        public async Task Invoke(HttpContext httpContext, ApiLogService apiLogService, ILogger<ApiLoggingMiddleware> logger)
        {
            _apiLogService = apiLogService;
            _logger = logger;

            try
            {
                var request = httpContext.Request;
                if (request.Path.StartsWithSegments(new PathString("/api")))
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();
                    var requestTime = DateTime.UtcNow;

                    //  Enable seeking
                    httpContext.Request.EnableBuffering();
                    //  Read the stream as text
                    var requestBodyContent = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                    //  Set the position of the stream to 0 to enable rereading
                    httpContext.Request.Body.Position = 0;

                    var originalBodyStream = httpContext.Response.Body;
                    using (var responseBody = new MemoryStream())
                    {
                        var response = httpContext.Response;
                        response.Body = responseBody;
                        await _next(httpContext);
                        stopWatch.Stop();

                        string responseBodyContent = null;
                        responseBodyContent = await ReadResponseBody(response);
                        await responseBody.CopyToAsync(originalBodyStream);

                        await SafeLog(requestTime,
                            stopWatch.ElapsedMilliseconds,
                            response.StatusCode,
                            request.Method,
                            request.Path,
                            request.QueryString.ToString(),
                            requestBodyContent,
                            responseBodyContent);
                    }
                }
                else
                {
                    await _next(httpContext);
                }
            }
            catch (Exception ex)
            {
                // We can't do anything if the response has already started, just abort.
                if (httpContext.Response.HasStarted)
                {
                    _logger.LogWarning("An exception occurred, but response has already started!");
                    throw;
                }

                await HandleExceptionAsync(httpContext, ex);
                throw;
            }
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
        }

        private async Task SafeLog(DateTime requestTime,
                            long responseMillis,
                            int statusCode,
                            string method,
                            string path,
                            string queryString,
                            string requestBody,
                            string responseBody)
        {
            // Do not log these events login, logout, getuserinfo...
            if (path.ToLower().StartsWith("/api/authorize/"))
            {
                return;
            }

            if (requestBody.Length > 256)
            {
                requestBody = $"(Truncated to 200 chars) {requestBody.Substring(0, 200)}";
            }

            if (responseBody.Length > 256)
            {
                responseBody = $"(Truncated to 200 chars) {responseBody.Substring(0, 200)}";
            }

            if (queryString.Length > 256)
            {
                queryString = $"(Truncated to 200 chars) {queryString.Substring(0, 200)}";
            }

            await _apiLogService.Log(new ApiLogItem
            {
                RequestTime = requestTime,
                ResponseMillis = responseMillis,
                StatusCode = statusCode,
                Method = method,
                Path = path,
                QueryString = queryString,
                RequestBody = requestBody,
                ResponseBody = responseBody
            });
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            _logger.LogError("Api Exception:", ex);

            httpContext.Response.Clear();
            httpContext.Response.OnStarting(_clearCacheHeadersDelegate, httpContext.Response);

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize<ClientApiResponse>(
              new ClientApiResponse {
                  StatusCode = httpContext.Response.StatusCode,
                  Message = "Internal Server Error from the custom middleware."
              }));
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
