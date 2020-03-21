using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlazorBoilerplate.Server.Middleware.Wrappers
{
    public class ApiError
    {
        public bool IsError { get; set; }
        public string ExceptionMessage { get; set; }
        public string Details { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }

        public ApiError(string message)
        {
            ExceptionMessage = message;
            IsError = true;
        }

        public ApiError(string message, IEnumerable<ValidationError> validationErrors)
        {
            ExceptionMessage = message;
            ValidationErrors = validationErrors;
        }

        public ApiError(ModelStateDictionary modelState)
        {
            IsError = true;
            if (modelState != null && modelState.Any(m => m.Value.Errors.Count > 0))
            {
                ExceptionMessage = "Please correct the specified validation errors and try again.";
                ValidationErrors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
                .ToList();
            }
        }
    }
}
