using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace BlazorBoilerplate.Server.Services
{

    public interface IEmailService
    {
        Task<(bool success, string errorMsg)> SendEmailAsync(EmailMessage emailMessage);

        List<EmailMessage> ReceiveEmail(int maxCount = 10);

        void Send(EmailMessage emailMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        // Logger instance
        ILogger<EmailService> _logger;

        public EmailService(IEmailConfiguration emailConfiguration, ILogger<EmailService> logger)
        {
            _emailConfiguration = emailConfiguration;
            _logger = logger;
        }

        public List<EmailMessage> ReceiveEmail(int maxCount = 10)
        {
            throw new NotImplementedException();
        }

        public void Send(EmailMessage emailMessage)
        {
            throw new NotImplementedException();
        }        

        public async Task<(bool success, string errorMsg)> SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Cc.AddRange(emailMessage.CcAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Bcc.AddRange(emailMessage.BccAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    message.To.Clear();
                    message.To.Add(new MailboxAddress("support@blazorboilerplate.com"));
                }

                message.Subject = emailMessage.Subject;

                message.Body = emailMessage.IsHtml ? new BodyBuilder { HtmlBody = emailMessage.Body }.ToMessageBody() : new TextPart("plain") { Text = emailMessage.Body };

                //TODO store all emails in Database

                //Be careful that the SmtpClient class is the one from Mailkit not the framework!
                using (var emailClient = new SmtpClient())
                {
                    if (!_emailConfiguration.SmtpUseSSL)
                    {
                        emailClient.ServerCertificateValidationCallback = (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                    }

                    await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, _emailConfiguration.SmtpUseSSL).ConfigureAwait(false);

                    //Remove any OAuth functionality as we won't be using it.
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (!string.IsNullOrWhiteSpace(_emailConfiguration.SmtpUsername))
                    {
                        await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword).ConfigureAwait(false);
                    }

                    await emailClient.SendAsync(message).ConfigureAwait(false);

                    await emailClient.DisconnectAsync(true).ConfigureAwait(false);
                    return (true, null);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Email Send Failed: {0}", ex.Message);
                return (false, ex.Message);
            }
        }
    }
}
