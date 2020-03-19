using System;
using System.Collections.Generic;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Middleware.Wrappers
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public bool IsModelValidatonError { get; set; }
        public IEnumerable<ValidationError> Errors { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }

        public ApiException(string message,
                            int statusCode = Status500InternalServerError,
                            string errorCode = "",
                            string refLink = "") :
            base(message)
        {
            IsModelValidatonError = false;
            StatusCode = statusCode;
            ReferenceErrorCode = errorCode;
            ReferenceDocumentLink = refLink;
        }

        public ApiException(IEnumerable<ValidationError> errors, int statusCode = Status400BadRequest)
        {
            IsModelValidatonError = true;
            StatusCode = statusCode;
            Errors = errors;
        }

        public ApiException(Exception ex, int statusCode = Status500InternalServerError) : base(ex.Message)
        {
            IsModelValidatonError = false;
            StatusCode = statusCode;
        }
    }
}
