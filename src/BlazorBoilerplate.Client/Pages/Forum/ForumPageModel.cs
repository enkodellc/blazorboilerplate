using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Client.Hubs;
using System.Collections.Generic;
using System;
using Microsoft.JSInterop;
using BlazorBoilerplate.Client.States;

namespace BlazorBoilerplate.Client.Pages
{
    public class ForumPageModel : ComponentBase
    {
        [Inject]
        private IdentityAuthenticationStateProvider IdentityAuthenticationStateProvider { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        UserInfoDto userInfo { get; set; } = new UserInfoDto();

        ChatClient client { get; set; }
        
        // on-screen message
        string message = null;

        public string MessagePost { get; set; }

        public List<Message> Messages = new List<Message>();

        //public ApiError Error { get; set; }
        
        protected override async Task OnInitializedAsync()
        {
            userInfo = await IdentityAuthenticationStateProvider.GetUserInfo();

            await AuthenticationStateTask;
            await GetMessages();
            await Chat();
        }
        
        public async Task SubmitMessage()
        {
            await GetMessages();
            MessagePost = "";
        }

        private async Task GetMessages()
        {
            //var result = await MessagesService.GetMessages();
            //
            //try
            //{
            //    if (result.Response != null)
            //    {
            //        Messages = result.Response.Value.ToList();
            //    }
            //    if (result.Error != null)
            //    {
            //        Error = result.Error;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Error = new ApiError() {
            //        Details = "Null result returned. The message service did not return any results.",
            //        Message = ex.Message
            //    };
            //}
        }


        /// <summary>
        /// Start chat client
        /// </summary>
        async Task Chat()
        {
            try
            {
                // remove old messages if any
                Messages.Clear();

                // Create the chat client
                client = new ChatClient(JsRuntime);

                // add an event handler for incoming messages
                client.MessageReceived += MessageReceived;

                // start the client
                Console.WriteLine($"Index: chart starting...");
                await client.Start();
                Console.WriteLine($"Index: chart started?");
            }
            catch (Exception e)
            {
                message = $"ERROR: Failed to start chat client: {e.Message}";
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }


        /// <summary>
        /// Inbound message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine($"Blazor: receive {e.Username}: {e.Message}");
            bool isMine = false;
            if (!string.IsNullOrWhiteSpace(e.Username))
            {
                isMine = string.Equals(e.Username, userInfo.UserName, StringComparison.CurrentCultureIgnoreCase);
            }

            var newMessage = new Message(e.Username, e.Message, isMine);
            Messages.Add(newMessage);

            // Inform blazor the UI needs updating
            StateHasChanged();
        }

        async Task Disconnect()
        {
            await client.Stop();
            client.Dispose();
            client = null;
            message = "chat ended";
        }

        public async Task Send(MessageDto messageDto)
        {
            if (!string.IsNullOrWhiteSpace(messageDto.Text))
            {
                //var result = await MessagesService.CreateMessage(messageDto);
                //if (result.Response != null)
                //{
                //    Messages.Add(result.Response);
                //}
                //if (result.Error != null)
                //{
                //    Error = result.Error;
                //}

                // send message to hub
                await client.Send(messageDto.Text);

                StateHasChanged();
            }
        }
    }

    public class Message
    {
        public Message(string username, string body, bool mine)
        {
            Username = username;
            Body = body;
            Mine = mine;
        }

        public string Username { get; set; }
        public string Body { get; set; }
        public bool Mine { get; set; }

        /// <summary>
        /// Determine CSS classes to use for message div
        /// </summary>
        public string CSS
        {
            get
            {
                return Mine ? "sent" : "received";
            }
        }
    }
}
