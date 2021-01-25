using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Email;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IEmailManager
    {
        Task<ApiResponse> SendTestEmail(EmailDto parameters);
        Task<ApiResponse> Receive();
        Task<ApiResponse> SendEmailAsync(EmailMessageDto emailMessage);
        List<EmailMessageDto> ReceiveEmail(int maxCount = 10);
        Task<ApiResponse> ReceiveMailImapAsync();
        Task<ApiResponse> ReceiveMailPopAsync(int min = 0, int max = 0);
    }
}