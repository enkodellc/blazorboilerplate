using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using BlazorBoilerplate.Singleton.TaskScheduler.Models;
using System.Text;
using BlazorBoilerplate.Shared.ApiContentService;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Singleton.TaskScheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceScopeFactory;
        
        public Worker(
            IServiceProvider serviceScopeFactory,
            ILogger<Worker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //wait for server to initialize database on first run
                await Task.Delay(1000 * 60 * 1, stoppingToken);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IApiContentProvider _ApiContentProvider = scope.ServiceProvider.GetRequiredService<IApiContentProvider>();
                    _logger.LogWarning("Task Scheduler running at: {time}", DateTimeOffset.Now);
                    var baseUri = "https://en.wikipedia.org";
                    var bearer = "bearer_token_obtained_from_api";
                    var client = new RestClient(baseUri);
                    client.Authenticator = new HttpBasicAuthenticator("Bearer", bearer);
                    var request = new RestRequest("/w/api.php?action=query&prop=revisions&rvprop=content&rvsection=0&titles=pizza&format=json&access_token=" + bearer, DataFormat.Json);
                    request.AddHeader("Accept", "application/json");
                    var response = client.Execute<Wiki>(request);
                    var objectPermissions = JsonConvert.DeserializeObject<Wiki>(response.Content);
                    StringBuilder sb = new StringBuilder();
                    bool hasUpdates = false;
                    foreach(var row in objectPermissions.Query.Pages)
                    {
                        if(await _ApiContentProvider.Upsert(row.Value))
                        {
                            hasUpdates = true;
                        }
                    }
                    if(hasUpdates)
                    {
                        await _ApiContentProvider.SaveChangesAsync();
                    }
                }
            }
        }
    }


    


}