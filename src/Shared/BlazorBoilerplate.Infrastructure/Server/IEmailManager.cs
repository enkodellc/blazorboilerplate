using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Email;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IEmailManager
    {
        Task<ApiResponse> Send(EmailDto parameters);
        Task<ApiResponse> Receive();
        Task<ApiResponse> SendEmailAsync(EmailMessageDto emailMessage);
        List<EmailMessageDto> ReceiveEmail(int maxCount = 10);
        Task<ApiResponse> ReceiveMailImapAsync();
        Task<ApiResponse> ReceiveMailPopAsync(int min = 0, int max = 0);
        void Send(EmailMessageDto emailMessage);
    }
}