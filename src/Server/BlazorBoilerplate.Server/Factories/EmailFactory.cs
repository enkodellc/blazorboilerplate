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

            emailMessage.Subject = string.Format("Hello {0} from BlazorBoilerplate Team", emailMessage.ToAddresses[0].Name);

            return emailMessage;
        }
        public EmailMessageDto GetPlainTextTestEmail(DateTime date)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["PlainTextTestEmail.template"].Value
                .FormatWith(new { date });

            emailMessage.IsHtml = false;

            return emailMessage;
        }
        public EmailMessageDto BuildNewUserConfirmationEmail(string recepientName, string userName, string callbackUrl, string userId, string token)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["NewUserConfirmationEmail.template"].Value
                .FormatWith(new { name = recepientName, userName, callbackUrl, userId, token });

            emailMessage.Subject = string.Format("Welcome {0} to BlazorBoilerplate", recepientName);

            return emailMessage;
        }
        public EmailMessageDto BuildNewUserEmail(string fullName, string userName, string emailAddress, string password)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["NewUserEmail.template"].Value
                .FormatWith(new { fullName = userName, userName, email = emailAddress, password });

            emailMessage.Subject = string.Format("Welcome {0} to BlazorBoilerplate", fullName);

            return emailMessage;
        }
        public EmailMessageDto BuilNewUserNotificationEmail(string creator, string name, string userName, string company, string roles)
        {
            var emailMessage = new EmailMessageDto();
            //placeholder not actually implemented

            emailMessage.Body = L["NewUserEmail.template"].Value
                .FormatWith(new { creator, name, userName, roles, company });

            emailMessage.Subject = string.Format("A new user '{0}' has registered on BlazorBoilerplate", userName);

            return emailMessage;
        }
        public EmailMessageDto BuildForgotPasswordEmail(string name, string callbackUrl, string token)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["ForgotPassword.template"].Value
                .FormatWith(new { name, callbackUrl, token });

            emailMessage.Subject = string.Format("BlazorBoilerplate Forgot Password Reset for {0}", name);

            return emailMessage;
        }
        public EmailMessageDto BuildPasswordResetEmail(string userName)
        {
            var emailMessage = new EmailMessageDto();

            emailMessage.Body = L["PasswordReset.template"].Value
                .FormatWith(new { userName });

            emailMessage.Subject = string.Format("BlazorBoilerplate Password Reset for {0}", userName);

            return emailMessage;
        }
    }
}
