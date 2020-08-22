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

        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;

        [DataMember]
        public string Message { get; set; }

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
        public ApiResponse(int statusCode, string message = "", object result = null, string apiVersion = "") : base(statusCode, message)
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
            Version = string.IsNullOrWhiteSpace(apiVersion) ? Assembly.GetEntryAssembly().GetName().Version.ToString() : apiVersion;
        }

        public ApiResponse(int statusCode) : base(statusCode, "")
        {
            StatusCode = statusCode;
        }
    }
}
