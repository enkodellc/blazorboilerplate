namespace BlazorBoilerplate.Infrastructure.Server.Models
{
    public class ApiError
    {
        public bool IsError { get; set; }
        public string ExceptionMessage { get; set; }
        public string Details { get; set; }

        public ApiError(string message)
        {
            ExceptionMessage = message;
            IsError = true;
        }
    }
}
