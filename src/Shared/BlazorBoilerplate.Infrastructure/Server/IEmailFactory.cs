using BlazorBoilerplate.Shared.Dto.Email;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IEmailFactory
    {
        EmailMessageDto BuildTestEmail(string recipient);
        EmailMessageDto GetPlainTextTestEmail(DateTime date);
        EmailMessageDto BuildNewUserConfirmationEmail(string fullName, string userName, string callbackUrl);
        EmailMessageDto BuildNewUserEmail(string fullName, string userName, string emailAddress, string password);
        EmailMessageDto BuilNewUserNotificationEmail(string creator, string name, string userName, string company, string roles);
        EmailMessageDto BuildForgotPasswordEmail(string name, string callbackUrl, string token);
        EmailMessageDto BuildPasswordResetEmail(string userName);
    }
}
