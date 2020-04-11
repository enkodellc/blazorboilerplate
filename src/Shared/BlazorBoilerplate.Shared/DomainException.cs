using System;


namespace BlazorBoilerplate.Shared
{
    public class DomainException : Exception
    {
        public string Description { get; }

        public DomainException(string description): base()
        {
            this.Description = description;
        }

        public DomainException(string description, string message) : base(message) {
            this.Description = description;

        }

        public DomainException(string description, string message, Exception innerException) : base(message, innerException) {
            this.Description = description;

        }
    }
}
