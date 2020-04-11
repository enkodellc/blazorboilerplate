using BlazorBoilerplate.CommonUI.Pages;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.Hubs
{

    /// <summary>
    /// Generic client class that interfaces .NET Standard/Blazor with SignalR Javascript client
    /// </summary>
    public class ChatClient : IDisposable
    {

        #region static methods

        /// <summary>
        /// internal dictionary of SignalR clients by Key
        /// </summary>
        private static readonly Dictionary<string, ChatClient> _clients = new Dictionary<string, ChatClient>();


        /// <summary>
        /// Inbound message handler 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="method"></param>
        /// <param name="username"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// This method is called from Javascript when amessage is received
        /// </remarks>
        [JSInvokable]
        public static void ReceiveMessage(string key, string method, int id, string username, string message)
        {
            if (_clients.ContainsKey(key))
            {
                var client = _clients[key];
                switch (method)
                {
                    case "ReceiveMessage":
                        client.HandleReceiveMessage(id, username, message);
                        return;

                    default:
                        throw new NotImplementedException(method);
                }
            }
            else
            {
                // unable to match the message to a client
                Console.WriteLine($"ReceiveMessage: unable to find {key}");
            }
        }

        #endregion

        /// <summary>
        /// Ctor: create a new client for the given hub URL
        /// </summary>
        /// <param name="hubUrl"></param>
        public ChatClient(IJSRuntime JSRuntime)
        {
            _JSruntime = JSRuntime;
            // create a unique key for this client
            _key = Guid.NewGuid().ToString();
            // add to the list of clients
            _clients.Add(_key, this);
        }

        /// <summary>
        /// The Hub URL for chat client
        /// </summary>
        const string HUBURL = "/chathub";

        /// <summary>
        /// Our unique key for this client instance
        /// </summary>
        /// <remarks>
        /// We cannot pass JS objects to Blazor/C# so we use a unique key
        /// to reference each instance. The JS client will store the object
        /// under our key so we can reference it
        /// </remarks>
        private readonly string _key;

        /// <summary>
        /// Name of the chatter
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// Flag to show if started
        /// </summary>
        private bool _started = false;

        /// <summary>
        /// JS runtime from DI
        /// </summary>
        private readonly IJSRuntime _JSruntime;


        /// <summary>
        /// Start the SignalR client on JS
        /// </summary>
        public async Task Start()
        {
            if (!_started)
            {
                // the callback method for inbound messages
                const string assembly = "BlazorBoilerplate.CommonUI";//TODO: Make this dynamic with the application
                const string method = "ReceiveMessage";
                // invoke the JS interop start client method
                Console.WriteLine("ChatClient: calling Start()");
                var _ = await _JSruntime.InvokeAsync<object>("ChatClient.Start", _key, HUBURL, assembly, method);
                Console.WriteLine("ChatClient: Start returned");
                _started = true;

                // register user
                await _JSruntime.InvokeAsync<object>("ChatClient.Register", _key, _username);
            }
        }

        /// <summary>
        /// Handle an inbound message from a hub
        /// </summary>
        /// <param name="method">event name</param>
        /// <param name="message">message content</param>
        private void HandleReceiveMessage(int id, string username, string message)
        {
            // raise an event to subscribers
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(id, username, message));
        }

        /// <summary>
        /// Event raised when this client receives a message
        /// </summary>
        /// <remarks>
        /// Instance classes should subscribe to this event
        /// </remarks>
        public event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// Send a message to the hub
        /// </summary>
        /// <param name="message">message to send</param>
        public async Task Send(string message)
        {
            // check we are connected
            if (!_started)
                throw new InvalidOperationException("Client not started");
            // send the message
            await _JSruntime.InvokeAsync<object>("ChatClient.Send", _key, message);
        }

        /// <summary>
        /// Delete a message from the hub by id
        /// </summary>
        /// <param name="id">message of id to be deleted</param>
        public async Task Delete(int id)
        {
            // check we are connected
            if (!_started)
                throw new InvalidOperationException("Client not started");
            // send the message
            await _JSruntime.InvokeAsync<object>("ChatClient.Delete", _key, id);
        }

        /// <summary>
        /// Stop the client (if started)
        /// </summary>
        public async Task Stop()
        {
            if (_started)
            {
                // disconnect the client
                await _JSruntime.InvokeAsync<object>("ChatClient.Stop", _key);
                _started = false;
            }
        }

        /// <summary>
        /// Dispose of client
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("ChatClient: Disposing");
            // ensure we stop if connected
            if (_started)
            {
                Task.Run(async () =>
                {
                    await Stop();
                }).Wait();
            }

            // remove this key from the list of clients
            if (_clients.ContainsKey(_key))
                _clients.Remove(_key);
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
