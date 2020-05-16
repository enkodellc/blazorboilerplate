namespace BlazorBoilerplate.Shared.Interfaces
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
}
