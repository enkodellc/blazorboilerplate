namespace BlazorBoilerplate.Shared.Dto.Email
{
    public class EmailDto
    {
        private string _name;
        public string ToAddress { get; set; }

        public string ToName
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return ToAddress;
                }

                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string FromName { get; set; }

        public string FromAddress { get; set; }

        public string ReplyToAddress { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string TemplateName { get; set; } = "Test";

        public bool Queued { get; set; }
    }
}
