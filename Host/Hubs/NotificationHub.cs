using Microsoft.AspNetCore.SignalR;

namespace Host.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        //public async Task SendIncidentUpdate(string incidentId, string updateType, object payload)
        //{
        //    await Clients.All.SendAsync("ReceiveIncidentUpdate", incidentId, updateType, payload);
        //}

        //public async Task SendChatMessage(Guid chatId, object message)
        //{
        //    await Clients.Group(chatId.ToString()).SendAsync("ReceiveChatMessage", message);
        //}

        //public async Task JoinResponderGroup()
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, "Responders");
        //}

        // public override async Task OnConnectedAsync()
        // {
        //     await base.OnConnectedAsync();
        //     await Clients.Caller.SendAsync("Connected", "Welcome to NaijaRescue Notification Hub 🚨");
        // }

        // public async Task SendNotification(string userId, string message)
        // {
        //     await Clients.User(userId).SendAsync("ReceiveNotification", message);
        // }
    }
}
