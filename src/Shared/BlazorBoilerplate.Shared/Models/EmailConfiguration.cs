namespace BlazorBoilerplate.Shared.Models
{
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpUseSSL { get; set; }

        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string ReplyToAddress { get; set; }

        public string PopServer { get; set; }
        public int PopPort { get; set; }
        public string PopUsername { get; set; }
        public string PopPassword { get; set; }
        public bool PopUseSSL { get; set; }

        public string ImapServer { get; set; }
        public int ImapPort { get; set; }
        public string ImapUsername { get; set; }
        public string ImapPassword { get; set; }
        public bool ImapUseSSL { get; set; }
    }
}
