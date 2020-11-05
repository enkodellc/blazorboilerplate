using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Shared.Dto.Email;
using FormatWith;
using Microsoft.Extensions.Localization;
using System;

namespace BlazorBoilerplate.Server.Factories
{
    public class EmailFactory : IEmailFactory
    {
        protected readonly IStringLocalizer<EmailFactory> L;
        public EmailFactory(IStringLocalizer<EmailFactory> l)
        {
            L = l;
        }

        public EmailMessageDto BuildTestEmail()
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["TestEmail.template"].Value
                .FormatWith(new { user = emailMessage.ToAddresses[0].Name, testDate = DateTime.Now });

            emailMessage.Subject = L["TestEmail.subject", emailMessage.ToAddresses[0].Name];

            return emailMessage;
        }
        public EmailMessageDto GetPlainTextTestEmail(DateTime date)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["PlainTextTestEmail.template"].Value
                .FormatWith(new { date });

            emailMessage.Subject = L["PlainTextTestEmail.subject", emailMessage.ToAddresses[0].Name];

            emailMessage.IsHtml = false;

            return emailMessage;
        }
        public EmailMessageDto BuildNewUserConfirmationEmail(string recepientName, string userName, string callbackUrl, string userId, string token)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["NewUserConfirmationEmail.template"].Value
                .FormatWith(new { name = recepientName, userName, callbackUrl, userId, token });

            emailMessage.Subject = L["NewUserConfirmationEmail.subject", recepientName];

            return emailMessage;
        }
        public EmailMessageDto BuildNewUserEmail(string fullName, string userName, string emailAddress, string password)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["NewUserEmail.template"].Value
                .FormatWith(new { fullName = userName, userName, email = emailAddress, password });

            emailMessage.Subject = L["NewUserEmail.subject", fullName];

            return emailMessage;
        }
        public EmailMessageDto BuilNewUserNotificationEmail(string creator, string name, string userName, string company, string roles)
        {
            var emailMessage = new EmailMessageDto();
            //placeholder not actually implemented

            emailMessage.Body = L["NewUserNotificationEmail.template"].Value
                .FormatWith(new { creator, name, userName, roles, company });

            emailMessage.Subject = L["NewUserNotificationEmail.subject", userName];

            return emailMessage;
        }
        public EmailMessageDto BuildForgotPasswordEmail(string name, string callbackUrl, string token)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["ForgotPassword.template"].Value
                .FormatWith(new { name, callbackUrl, token });

            emailMessage.Subject = L["ForgotPassword.subject", name];

            return emailMessage;
        }
        public EmailMessageDto BuildPasswordResetEmail(string userName)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["PasswordReset.template"].Value
                .FormatWith(new { userName });

            emailMessage.Subject = L["PasswordReset.subject", userName];

            return emailMessage;
        }
    }
}
