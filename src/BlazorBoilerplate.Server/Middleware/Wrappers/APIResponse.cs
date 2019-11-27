using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace BlazorBoilerplate.Server.Middleware.Wrappers
{
    [Serializable]
    [DataContract]
    public class ApiResponse
    {
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public int StatusCode { get; set; } = 0;

        [DataMember]
        public bool IsError { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ApiError ResponseException { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Result { get; set; }

        [JsonConstructor]
        public ApiResponse(int statusCode, string message = "", object result = null, ApiError apiError = null, string apiVersion = "0.3.2")
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Result = result;
            this.ResponseException = apiError;
            this.Version = apiVersion;
            this.IsError = false;
        }

        public ApiResponse(int statusCode, ApiError apiError)
        {
            this.StatusCode = statusCode;
            this.Message = apiError.ExceptionMessage;
            this.ResponseException = apiError;
            this.IsError = true;
        }
    }
}
