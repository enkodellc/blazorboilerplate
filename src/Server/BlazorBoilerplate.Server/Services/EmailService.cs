using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Services
{
    public class EmailService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailService> _logger;
        public EmailService(
            IServiceScopeFactory scopeFactory,
            ILogger<EmailService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Program.Sync.WaitAsync(stoppingToken);                

                _logger.LogInformation($"EmailService starting...");

                do
                {
                    using var scope = _scopeFactory.CreateScope();
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var emailManager = scope.ServiceProvider.GetRequiredService<IEmailManager>();

                        foreach (var email in dbContext.QueuedEmails.Where(i => i.SentOn == null).OrderBy(i => i.CreatedOn))
                        {
                            var response = await emailManager.SendEmail(JsonConvert.DeserializeObject<EmailMessageDto>(email.Email));

                            if (response.IsSuccessStatusCode)
                            {
                                email.SentOn = DateTime.Now;

                                await dbContext.SaveChangesAsync(stoppingToken);
                            }

                            if (stoppingToken.IsCancellationRequested)
                                return;
                        }
                    }

                    await EmailManager.QueueSync.WaitAsync(60000, stoppingToken);

                } while (!stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex)
            {
                _logger.LogError($"EmailService: ExecuteAsync {ex.GetBaseException()}");
            }
        }
    }
}
