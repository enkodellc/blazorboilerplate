using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Server.Models
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
            CcAddresses = new List<EmailAddress>();
            BccAddresses = new List<EmailAddress>();
        }

        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public List<EmailAddress> BccAddresses { get; set; }
        public List<EmailAddress> CcAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public bool IsHtml { get; set; } = true;
    }
}
