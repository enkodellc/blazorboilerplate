using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorBoilerplate.Theme.Material.Demo.Hubs
{

    /// <summary>
    /// Generic client class that interfaces .NET Standard/Blazor with SignalR .NET Core client
    /// </summary>
    public class ChatClient : IDisposable
    {
        private readonly HubConnection connection;

        /// <summary>
        /// Event raised when this client receives a message
        /// </summary>
        /// <remarks>
        /// Instance classes should subscribe to this event
        /// </remarks>
        public event MessageReceivedEventHandler MessageReceived;

        public ChatClient(HttpClient httpClient)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(httpClient.BaseAddress, "chathub"), options =>
                {
                    foreach (var header in httpClient.DefaultRequestHeaders)
                        if (header.Key == "Cookie")
                            options.Headers.Add(header.Key, header.Value.First());
                })
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task Start()
        {
            connection.On<int, string, string>("ReceiveMessage", (id, username, message) =>
            {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(id, username, message));
            });

            await connection.StartAsync();
        }

        public async Task Send(string message)
        {
            await connection.InvokeAsync("SendMessage", message);
        }
        public async Task Delete(int id)
        {
            await connection.InvokeAsync("DeleteMessage", id);
        }
        public async Task Stop()
        {
            if (connection.State != HubConnectionState.Disconnected)
            {
                await connection.StopAsync();
            }
        }

        /// <summary>
        /// Dispose of client
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("ChatClient: Disposing");

            if (connection.State != HubConnectionState.Disconnected)
            {
                Task.Run(async () =>
                {
                    await Stop();
                }).Wait();
            }
        }
    }

    /// <summary>
    /// Delegate for the message handler
    /// </summary>
    /// <param name="sender">the SignalRclient instance</param>
    /// <param name="e">Event args</param>
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    /// <summary>
    /// Message received argument class
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(int id, string username, string message)
        {
            Id = id;
            Username = username;
            Message = message;
        }

        /// <summary>
        /// Name of the message/event
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Message data items
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Message id
        /// </summary>
        public int Id { get; internal set; }
    }
}
