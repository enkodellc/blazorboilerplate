namespace BlazorBoilerplate.Server.Models
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }
        bool SmtpUseSSL { get; set; }
        string FromName { get; set; }
        string FromAddress { get; set; }
        string ReplyToAddress { get; set; }

        string PopServer { get; }
        int PopPort { get; }
        string PopUsername { get; }
        string PopPassword { get; }
        bool PopUseSSL { get; }

        string ImapServer { get; set; }
        int ImapPort { get; set; }
        string ImapUsername { get; set; }
        string ImapPassword { get; set; }
        bool ImapUseSSL { get; set; }
    }

    public class EmailConfiguration : IEmailConfiguration
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
