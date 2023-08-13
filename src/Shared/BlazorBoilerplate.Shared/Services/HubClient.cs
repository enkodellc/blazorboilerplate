using BlazorBoilerplate.Constants;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorBoilerplate.Shared.Services
{
    public class HubClient : IDisposable
    {
        private readonly HubConnection connection;

        public event OnlineUsersReceivedEventHandler OnlineUsersReceived;
        public event LongOperationResultReceivedEventHandler LongOperationResultReceived;

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
                connection.On<Guid, bool>("NotifyOnlineUsers", (userId, online) =>
                {
                    OnlineUsersReceived?.Invoke(this, new OnlineUsersReceivedEventArgs(userId, online));
                });

                connection.On<string, bool>("NotifyLongOperationCompleted", (message, success) =>
                {
                    LongOperationResultReceived?.Invoke(this, new LongOperationResultReceivedEventArgs(message, success));
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

        public async Task<IEnumerable<Guid>> GetOnlineUsers()
        {
            return await connection.InvokeAsync<IEnumerable<Guid>>("GetOnlineUsers");
        }

        public void Dispose()
        {
            if (connection.State != HubConnectionState.Disconnected)
            {
                Task.Run(Stop).Wait();
            }
        }
    }
    

    public delegate void LongOperationResultReceivedEventHandler(object sender, LongOperationResultReceivedEventArgs e);

    public class LongOperationResultReceivedEventArgs : EventArgs
    {
        public LongOperationResultReceivedEventArgs(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public delegate void OnlineUsersReceivedEventHandler(object sender, OnlineUsersReceivedEventArgs e);
    public class OnlineUsersReceivedEventArgs : EventArgs
    {
        public OnlineUsersReceivedEventArgs(Guid userId, bool online)
        {
            UserId = userId;
            Online = online;
        }

        public Guid UserId { get; set; }
        public bool Online { get; set; }
    }
}
