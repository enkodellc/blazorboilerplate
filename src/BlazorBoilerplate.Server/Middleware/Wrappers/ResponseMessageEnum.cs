using System.ComponentModel;

namespace BlazorBoilerplate.Server.Middleware.Wrappers
{
    public enum ResponseMessageEnum
    {
        [Description("Request successful.")]
        Success,
        [Description("Request responded with exceptions.")]
        Exception,
        [Description("Request denied.")]
        UnAuthorized,
        [Description("Request responded with validation error(s).")]
        ValidationError,
        [Description("Unable to process the request.")]
        Failure
    }
}
