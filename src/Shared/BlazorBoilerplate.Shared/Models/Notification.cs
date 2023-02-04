namespace BlazorBoilerplate.Shared.Models
{
    public class Notification
    {
        public NotificationType NotificationType { get; set; }

        public string Value { get; set; }

        public bool Success { get; set; }

        public override string ToString()
        {
            return $"Notification: {NotificationType} {Value}";
        }
    };
}
