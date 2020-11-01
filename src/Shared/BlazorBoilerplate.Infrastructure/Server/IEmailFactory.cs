using BlazorBoilerplate.Shared.Dto.Email;
using System;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IEmailFactory
    {
        EmailMessageDto BuildTestEmail();
        EmailMessageDto GetPlainTextTestEmail(DateTime date);
        EmailMessageDto BuildNewUserConfirmationEmail(string recepientName, string userName, string callbackUrl, string userId, string token);
        EmailMessageDto BuildNewUserEmail(string fullName, string userName, string emailAddress, string password);
        EmailMessageDto BuilNewUserNotificationEmail(string creator, string name, string userName, string company, string roles);
        EmailMessageDto BuildForgotPasswordEmail(string name, string callbackUrl, string token);
        EmailMessageDto BuildPasswordResetEmail(string userName);
    }
}
