namespace BlazorBoilerplate.Infrastructure.Server.Models
{
    public class DomainException : Exception
    {
        public string Description { get; }

        public DomainException(string description) : base()
        {
            Description = description;
        }

        public DomainException(string description, string message) : base(message)
        {
            Description = description;

        }

        public DomainException(string description, string message, Exception innerException) : base(message, innerException)
        {
            Description = description;

        }
    }
}
