using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorBoilerplate.Shared.Services
{
    public class HubClient : IDisposable
    {
        private readonly HubConnection connection;

        public event NotificationReceivedEventHandler NotificationReceived;

        public HubClient(HttpClient httpClient)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(httpClient.BaseAddress, HubPaths.Main), options =>
                {
                    foreach (var header in httpClient.DefaultRequestHeaders)
                        if (header.Key == "Cookie")
                            options.Headers.Add(header.Key, header.Value.First());
                })
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task<bool> Start()
        {
            var result = false;

            if (connection.State == HubConnectionState.Disconnected)
            {
                connection.On<Notification, string>("Notify", (notification, sender) =>
                {
                    NotificationReceived?.Invoke(this, new NotificationReceivedEventArgs(notification, sender));
                });

                await connection.StartAsync();
            }

            result = connection.State != HubConnectionState.Disconnected;

            return result;
        }
        public async Task Stop()
        {
            if (connection.State != HubConnectionState.Disconnected)
            {
                await connection.StopAsync();
            }
        }

        public void Dispose()
        {
            if (connection.State != HubConnectionState.Disconnected)
            {
                Task.Run(Stop).Wait();
            }
        }
    }

    public delegate void NotificationReceivedEventHandler(object sender, NotificationReceivedEventArgs e);

    public class NotificationReceivedEventArgs : EventArgs
    {
        public NotificationReceivedEventArgs(Notification notification, string sender = null)
        {
            Notification = notification;
            Sender = sender;
        }

        public Notification Notification { get; set; }
        public string Sender { get; set; }
    }
}
