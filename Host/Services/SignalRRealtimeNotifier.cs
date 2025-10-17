using Application.Common.Interfaces.Realtime;
using Microsoft.AspNetCore.SignalR;
using Host.Hubs;

namespace Host.Services
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRRealtimeNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(Guid userId, string method, object payload)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync(method, payload);
        }

        public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string method, object payload)
        {
            await _hubContext.Clients.Users(userIds.Select(x => x.ToString())).SendAsync(method, payload);
        }

        public async Task BroadcastAsync(string method, object payload)
        {
            await _hubContext.Clients.All.SendAsync(method, payload);
        }
    }
}
