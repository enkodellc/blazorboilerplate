using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Server.Aop;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Storage;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using Newtonsoft.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    [ApiResponseException]
    public class EmailManager : IEmailManager
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly IEmailFactory _emailFactory;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EmailManager> _logger;

        public static SemaphoreSlim QueueSync { get; private set; } = new SemaphoreSlim(0, 1);

        public EmailManager(
            ITenantSettings<EmailConfiguration> emailConfiguration,
            IEmailFactory emailFactory,
            ApplicationDbContext dbContext,
            ILogger<EmailManager> logger)
        {
            _emailConfiguration = emailConfiguration.Value;
            _emailFactory = emailFactory;
            _dbContext = dbContext;
            _logger = logger;
        }

        //Used by API
        public async Task<ApiResponse> SendTestEmail(EmailDto parameters)
        {
            EmailMessageDto email = _emailFactory.BuildTestEmail(parameters.ToName);
            email.ToAddresses.Add(new EmailAddressDto(parameters.ToName, parameters.ToAddress));

            return parameters.Queued ? await QueueEmail(email, EmailType.Test) : await SendEmail(email);
        }

        public Task<ApiResponse> Receive()
        {
            throw new NotImplementedException();
        }

        public List<EmailMessageDto> ReceiveEmail(int maxCount = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse> ReceiveMailImapAsync()
        {
            using var emailClient = new ImapClient();
            // use this if you need to specify using ssl; MailKit should usually be able to autodetect the appropriate settings
            // await emailClient.ConnectAsync(_emailConfiguration.ImapServer, _emailConfiguration.ImapPort, _emailConfiguration.ImapUseSSL);

            await emailClient.ConnectAsync(_emailConfiguration.ImapServer, _emailConfiguration.ImapPort);

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");


            if (!string.IsNullOrWhiteSpace(_emailConfiguration.ImapUsername))
            {
                await emailClient.AuthenticateAsync(_emailConfiguration.ImapUsername, _emailConfiguration.ImapPassword);
            }

            List<EmailMessageDto> emails = new();
            await emailClient.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            //TODO implement email results filtering
            var uids = await emailClient.Inbox.SearchAsync(SearchQuery.All);
            foreach (var uid in uids)
            {
                var message = await emailClient.Inbox.GetMessageAsync(uid);

                var emailMessage = new EmailMessageDto
                {
                    Body = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,

                    Subject = message.Subject
                };
                emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddressDto(x.Name, x.Address)));
                emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddressDto(x.Name, x.Address)));

                emails.Add(emailMessage);
            }

            await emailClient.DisconnectAsync(true);
            return new ApiResponse(Status200OK, null, emails);
        }

        public async Task<ApiResponse> ReceiveMailPopAsync(int min = 0, int max = 0)
        {
            using var emailClient = new Pop3Client();
            await emailClient.ConnectAsync(_emailConfiguration.PopServer, _emailConfiguration.PopPort);     // omitting usessl to allow mailkit to autoconfigure

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            if (!String.IsNullOrWhiteSpace(_emailConfiguration.PopUsername))
            {
                await emailClient.AuthenticateAsync(_emailConfiguration.PopUsername, _emailConfiguration.PopPassword);
            }

            List<EmailMessageDto> emails = new();

            if (max == 0) max = await emailClient.GetMessageCountAsync(); // if max not defined, get all messages

            for (int i = min; i < max; i++)
            {
                var message = await emailClient.GetMessageAsync(i);

                var emailMessage = new EmailMessageDto
                {
                    Body = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
                    IsHtml = !string.IsNullOrEmpty(message.HtmlBody),
                    Subject = message.Subject

                };
                emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddressDto(x.Name, x.Address)));
                emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddressDto(x.Name, x.Address)));

                emails.Add(emailMessage);
            }

            await emailClient.DisconnectAsync(true);
            return new ApiResponse(Status200OK, null, emails);
        }

        public async Task<ApiResponse> QueueEmail(EmailMessageDto emailMessage, EmailType emailType)
        {
            try
            {
                _dbContext.QueuedEmails.Add(new QueuedEmail() { Email = JsonConvert.SerializeObject(emailMessage), EmailType = emailType });

                await _dbContext.SaveChangesAsync();

                QueueSync.Release();

                var msg = $"Email to {string.Join(" - ", emailMessage.ToAddresses.Select(i => i.Address))} queued";

                _logger.LogInformation(msg);

                return new ApiResponse(Status200OK, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError($"QueueEmail failed {ex.GetBaseException().Message} {ex.StackTrace}");

                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }

        public async Task<ApiResponse> SendEmail(EmailMessageDto emailMessage)
        {
            try
            {
                var message = new MimeMessage();

                if (emailMessage.FromAddresses.Count == 0)
                    emailMessage.FromAddresses.Add(new EmailAddressDto(_emailConfiguration.FromName, _emailConfiguration.FromAddress));

                message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Cc.AddRange(emailMessage.CcAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Bcc.AddRange(emailMessage.BccAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

                message.Sender = new MailboxAddress(_emailConfiguration.FromName, _emailConfiguration.FromAddress);

                message.Subject = emailMessage.Subject;

                message.Body = emailMessage.IsHtml ? new BodyBuilder { HtmlBody = emailMessage.Body }.ToMessageBody() : new TextPart("plain") { Text = emailMessage.Body };

                //Be careful that the SmtpClient class is the one from Mailkit not the framework!
                using var emailClient = new SmtpClient();

                if (_emailConfiguration.SmtpUseSSL)
                    emailClient.SslProtocols = _emailConfiguration.SmtpSslProtocol;
                else
                    emailClient.ServerCertificateValidationCallback = (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;


                await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, _emailConfiguration.SmtpUseSSL);

                //Remove any OAuth functionality as we won't be using it.
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                if (!string.IsNullOrWhiteSpace(_emailConfiguration.SmtpUsername))
                {
                    await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                }

                await emailClient.SendAsync(message);

                await emailClient.DisconnectAsync(true);

                var msg = $"Email successfully sent to {string.Join(" - ", message.To.Select(i => i.Name))}";

                _logger.LogInformation(msg);

                return new ApiResponse(Status200OK, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError("SendEmail failed ({0}:{1} SSL:{2}): {3} {4}",
                    _emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, _emailConfiguration.SmtpUseSSL, ex.GetBaseException().Message, ex.StackTrace);

                return new ApiResponse(Status500InternalServerError, ex.GetBaseException().Message);
            }
        }
    }
}
