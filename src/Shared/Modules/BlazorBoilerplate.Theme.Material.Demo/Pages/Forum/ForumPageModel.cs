using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using BlazorBoilerplate.Theme.Material.Demo.Hubs;
using System.Collections.Generic;
using System;
using Microsoft.JSInterop;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Dto.Sample;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorBoilerplate.Shared.Providers;

namespace BlazorBoilerplate.Theme.Material.Demo.Pages
{
    public class ForumPageModel : ComponentBase
    {
        [Inject]
        private AuthenticationStateProvider authStateProvider { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        UserViewModel userViewModel { get; set; } = new UserViewModel();

        ChatClient Client { get; set; }

        // on-screen message (TODO:this is used for toast messages)
        string message = null;

        public string MessagePost { get; set; }

        public List<MessageDto> Messages = new List<MessageDto>();
        
        protected override async Task OnInitializedAsync()
        {
            userViewModel = await ((IdentityAuthenticationStateProvider)authStateProvider).GetUserViewModel();

            await AuthenticationStateTask;
            await Chat();
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
                Client = new ChatClient(JsRuntime);

                // add an event handler for incoming messages
                Client.MessageReceived += MessageReceived;

                // start the client
                //Console.WriteLine($"Index: chart starting...");
                await Client.Start();
                //Console.WriteLine($"Index: chart started?");
            }
            catch (Exception e)
            {
                message = $"ERROR: Failed to start chat client: {e.Message}";
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
            }
        }


        /// <summary>
        /// Inbound message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Console.WriteLine($"Blazor: receive {e.Username}: {e.Message}");
            bool isMine = false;
            if (!string.IsNullOrWhiteSpace(e.Username))
            {
                isMine = string.Equals(e.Username, userViewModel.UserName, StringComparison.CurrentCultureIgnoreCase);
            }

            var newMessage = new MessageDto(e.Id, e.Username, e.Message, isMine);
            Messages.Insert(0, newMessage);

            // Inform blazor the UI needs updating
            StateHasChanged();
        }

        async Task Disconnect()
        {
            await Client.Stop();
            Client.Dispose();
            Client = null;
            message = "chat ended";
        }

        public async Task Delete(MessageDto messageDto)
        {
            if (messageDto != null)
            {
                // send id to hub
                await Client.Delete(messageDto.Id);

                Messages.Remove(messageDto);

                StateHasChanged();
            }
        }

        public async Task Send(MessageDto messageDto)
        {
            if (!string.IsNullOrWhiteSpace(messageDto.Text))
            {
                // send message to hub
                await Client.Send(messageDto.Text);

                StateHasChanged();
            }
        }
    }
}
