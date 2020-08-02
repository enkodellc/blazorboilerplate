using BlazorBoilerplate.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace BlazorBoilerplate.Infrastructure.Server.Models
{
    [Serializable]
    [DataContract]
    public class ApiResponse<T>
    {
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ApiError ResponseException { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public T Result { get; set; }

        [JsonConstructor]
        public ApiResponse(int statusCode, string message = "")
        {
            StatusCode = statusCode;
            Message = message;
        }
    }

    [Serializable]
    [DataContract]
    public class ApiResponse : ApiResponse<object>
    {
        [JsonConstructor]
        public ApiResponse(int statusCode, string message = "", object result = null, ApiError apiError = null, string apiVersion = "", PaginationDetails paginationDetails = null) : base(statusCode, message)
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
            ResponseException = apiError;
            Version = string.IsNullOrWhiteSpace(apiVersion) ? Assembly.GetEntryAssembly().GetName().Version.ToString() : apiVersion;
        }

        public ApiResponse(int statusCode, ApiError apiError) : base(statusCode, "")
        {
            StatusCode = statusCode;
            Message = apiError.ExceptionMessage;
            ResponseException = apiError;
        }
    }
}
