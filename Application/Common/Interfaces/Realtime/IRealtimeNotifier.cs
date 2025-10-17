namespace Application.Common.Interfaces.Realtime
{
    public interface IRealtimeNotifier
    {
        Task SendToUserAsync(Guid userId, string method, object payload);
        Task BroadcastAsync(string method, object payload);
        Task SendToUsersAsync(IEnumerable<Guid> userIds, string method, object payload);
    }
}
