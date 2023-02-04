namespace BlazorBoilerplate.Server.Services
{
    public class BackgroundWorkerQueueService : BackgroundService
    {
        private readonly BackgroundWorkerQueue queue;
        public static IServiceScopeFactory ScopeFactory;

        public BackgroundWorkerQueueService(BackgroundWorkerQueue queue, IServiceScopeFactory scopeFactory)
        {
            this.queue = queue;
            ScopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await queue.DequeueAsync(stoppingToken);

                await workItem(stoppingToken);
            }
        }
    }
}
