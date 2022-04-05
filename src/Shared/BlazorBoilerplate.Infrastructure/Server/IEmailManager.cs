using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Email;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IEmailManager
    {
        Task<ApiResponse> SendTestEmail(EmailDto parameters);
        Task<ApiResponse> Receive();
        Task<ApiResponse> QueueEmail(EmailMessageDto emailMessage, EmailType emailType);
        Task<ApiResponse> SendEmail(EmailMessageDto emailMessage);
        List<EmailMessageDto> ReceiveEmail(int maxCount = 10);
        Task<ApiResponse> ReceiveMailImapAsync();
        Task<ApiResponse> ReceiveMailPopAsync(int min = 0, int max = 0);
    }
}